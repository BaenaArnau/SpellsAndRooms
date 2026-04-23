using Godot;
using System.Collections.Generic;
using System.Linq;
using SpellsAndRooms.scripts.Characters;

namespace SpellsAndRooms.scripts.Turns
{
    public partial class BattleScene : Node
    {
        [Signal] public delegate void BattleFinishedEventHandler(bool playerWon, int earnedGold);

        private readonly BattleController _battleController = new BattleController();
        private Player _player;
        private List<Enemy> _enemies = new List<Enemy>();
        private Skill _pendingSkill;
        private bool _battleEnded;

        private Label _statusLabel;
        private RichTextLabel _logLabel;
        private HBoxContainer _actionsContainer;
        private HBoxContainer _targetsContainer;

        public void StartBattle(Player player, List<Enemy> enemies)
        {
            _player = player;
            _enemies = enemies ?? new List<Enemy>();

            if (IsInsideTree())
            {
                BuildUi();
                StartPlayerTurn();
            }
        }

        public override void _Ready()
        {
            BuildUi();
            if (_player != null)
            {
                StartPlayerTurn();
            }
        }

        private void BuildUi()
        {
            if (GetNodeOrNull<CanvasLayer>("CanvasLayer") != null)
            {
                RefreshStatus();
                return;
            }

            var layer = new CanvasLayer { Name = "CanvasLayer" };
            AddChild(layer);

            var root = new Control
            {
                Name = "Root",
                AnchorRight = 1,
                AnchorBottom = 1,
                MouseFilter = Control.MouseFilterEnum.Stop
            };
            layer.AddChild(root);

            var dim = new ColorRect
            {
                AnchorRight = 1,
                AnchorBottom = 1,
                Color = new Color(0, 0, 0, 0.78f)
            };
            root.AddChild(dim);

            var panel = new PanelContainer
            {
                AnchorLeft = 0.08f,
                AnchorTop = 0.08f,
                AnchorRight = 0.92f,
                AnchorBottom = 0.92f
            };
            root.AddChild(panel);

            var layout = new VBoxContainer
            {
                Name = "Layout",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            panel.AddChild(layout);

            _statusLabel = new Label { Name = "Status" };
            layout.AddChild(_statusLabel);

            _actionsContainer = new HBoxContainer { Name = "Actions" };
            layout.AddChild(_actionsContainer);

            _targetsContainer = new HBoxContainer { Name = "Targets" };
            layout.AddChild(_targetsContainer);

            _logLabel = new RichTextLabel
            {
                Name = "Log",
                BbcodeEnabled = false,
                FitContent = true,
                ScrollFollowing = true,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            layout.AddChild(_logLabel);

            RefreshStatus();
        }

        private void StartPlayerTurn()
        {
            if (_battleEnded)
            {
                return;
            }

            RefreshStatus();
            RebuildActionButtons();
            RebuildTargetButtons(false);
            AddLog("Tu turno: elige una accion.");
        }

        private void RebuildActionButtons()
        {
            foreach (Node child in _actionsContainer.GetChildren())
            {
                child.QueueFree();
            }

            Button attackButton = CreateButton("Ataque");
            attackButton.Pressed += () =>
            {
                _pendingSkill = null;
                RebuildTargetButtons(true);
                AddLog("Elige objetivo para el ataque basico.");
            };
            _actionsContainer.AddChild(attackButton);

            if (_player == null)
            {
                return;
            }

            foreach (Skill skill in _player.Skills)
            {
                Button skillButton = CreateButton($"{skill.Name} ({skill.ManaCost} MP)");
                skillButton.Disabled = _player.Mana < skill.ManaCost;
                skillButton.Pressed += () => OnSkillSelected(skill);
                _actionsContainer.AddChild(skillButton);
            }
        }

        private void OnSkillSelected(Skill skill)
        {
            if (_battleEnded || _player == null)
            {
                return;
            }

            _pendingSkill = skill;
            if (skill.IsHealing)
            {
                AddLog(_battleController.PlayerUseSkill(_player, skill));
                RefreshStatus();
                CheckBattleEndOrEnemyTurn();
                return;
            }

            RebuildTargetButtons(true);
            AddLog($"Elige objetivo para {skill.Name}.");
        }

        private void RebuildTargetButtons(bool visible)
        {
            _targetsContainer.Visible = visible;

            foreach (Node child in _targetsContainer.GetChildren())
            {
                child.QueueFree();
            }

            if (!visible)
            {
                return;
            }

            foreach (Enemy enemy in _enemies.Where(e => e.IsAlive))
            {
                Button targetButton = CreateButton($"{enemy.CharacterName} HP:{enemy.Health}");
                targetButton.Pressed += () => OnTargetSelected(enemy);
                _targetsContainer.AddChild(targetButton);
            }
        }

        private void OnTargetSelected(Enemy enemy)
        {
            if (_battleEnded || _player == null || enemy == null || !enemy.IsAlive)
            {
                return;
            }

            string log;
            if (_pendingSkill == null)
            {
                log = _battleController.PlayerBasicAttack(_player, enemy);
            }
            else
            {
                log = _battleController.PlayerUseSkill(_player, _pendingSkill, enemy);
            }

            _pendingSkill = null;
            AddLog(log);
            RefreshStatus();
            CheckBattleEndOrEnemyTurn();
        }

        private async void CheckBattleEndOrEnemyTurn()
        {
            if (TryFinishBattle())
            {
                return;
            }

            ToggleActionButtons(false);
            await ToSignal(GetTree().CreateTimer(0.35), SceneTreeTimer.SignalName.Timeout);

            foreach (string enemyLog in _battleController.ExecuteEnemyTurn(_enemies, _player))
            {
                AddLog(enemyLog);
            }

            RefreshStatus();
            if (TryFinishBattle())
            {
                return;
            }

            ToggleActionButtons(true);
            StartPlayerTurn();
        }

        private bool TryFinishBattle()
        {
            if (_battleEnded || _player == null)
            {
                return true;
            }

            bool enemiesAlive = _battleController.HasAliveEnemies(_enemies);
            if (_player.IsAlive && enemiesAlive)
            {
                return false;
            }

            _battleEnded = true;
            BattleResult result = _battleController.BuildResult(_player, _enemies);
            AddLog(result.PlayerWon
                ? $"Victoria. Oro ganado: {result.EarnedGold}. Oro total: {_player.Gold}."
                : "Derrota. El equipo ha caido.");

            EmitSignal(SignalName.BattleFinished, result.PlayerWon, result.EarnedGold);
            QueueFree();
            return true;
        }

        private void RefreshStatus()
        {
            if (_statusLabel == null || _player == null)
            {
                return;
            }

            string enemySummary = string.Join(" | ", _enemies.Select(e => $"{e.CharacterName}:{e.Health}HP"));
            _statusLabel.Text = $"Jugador HP:{_player.Health}/{_player.BaseHealth} MP:{_player.Mana}/{_player.BaseMana}  ||  {enemySummary}";
        }

        private void ToggleActionButtons(bool enabled)
        {
            foreach (Button button in _actionsContainer.GetChildren().OfType<Button>())
            {
                button.Disabled = !enabled;
            }

            foreach (Button button in _targetsContainer.GetChildren().OfType<Button>())
            {
                button.Disabled = !enabled;
            }
        }

        private void AddLog(string line)
        {
            if (_logLabel == null || string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            _logLabel.AppendText(line + "\n");
        }

        private static Button CreateButton(string text)
        {
            return new Button
            {
                Text = text,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
        }
    }
}

