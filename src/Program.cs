using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;

namespace IngameScript 
{    
    partial class Program : MyGridProgram
    {
        // The Warhead detonation time,  0 < Time < 3600
        private int TIME = 20;

        // 1 = Script runs every 100 ticks, 10 Script, needs to be bigger than 0
        private const int TICK_MULTIPLIER = 10;

        // The LCD debug screen
        private const string LCD = "Boom LCD";

        private int _tickMultiplier = 0;

        private const UpdateType COMMAND_UPDATE = UpdateType.Trigger | UpdateType.Terminal;
        private IMyTextPanel _LogOutput;

        private IMyTerminalBlock _CheckBlock;
        private List<IMyWarhead> _Warheads = new List<IMyWarhead>();
        private bool countdown = false;

        public Program()
        {
            try
            {
                // Replace the Echo

                // Fetch a log text panel
                _LogOutput = GridTerminalSystem.GetBlockWithName(LCD) as IMyTextPanel;
                if (_LogOutput != null)
                {
                    _LogOutput.BackgroundColor = new Color(0, 0, 0, 255);
                    _LogOutput.FontColor = new Color(0, 255, 0, 255);
                    _LogOutput.FontSize = 0.8f;

                    Echo = EchoToLCD;
                }

                _LogOutput?.WriteText("");
                Echo("");

                if (TIME < 1 || TIME > 3600 || TICK_MULTIPLIER < 1)
                {
                    throw new InvalidOperationException("_DetonationTime or tick multiplier is out of bounds");
                }

                _CheckBlock = GridTerminalSystem.GetBlockWithName(Me.CustomData);
                if (_CheckBlock == null) {
                    throw new NullReferenceException("Exception: Master block not found");
                }

                _tickMultiplier = TICK_MULTIPLIER;
                Runtime.UpdateFrequency = UpdateFrequency.Update100;
            }
            catch (NullReferenceException e)
            {
                Echo($"Exception: {e}\n---");
                throw;
            }
            catch (InvalidOperationException e)
            {
                Echo($"Exception: {e}\n---");
                throw;
            }
            catch (Exception e)
            {
                Echo($"Exception: {e}\n---");
                throw;
            }
        }

        public void Main(string argument, UpdateType updateType)
        {
            _tickMultiplier++;
            if (_tickMultiplier > TICK_MULTIPLIER) {
                try
                {
                    if ((updateType & COMMAND_UPDATE) != 0)
                    {
                        RunCommand(argument);
                    } 
                    if ((updateType & UpdateType.Update100) != 0)
                    {
                        RunContinuousLogic();
                    }
                }
                catch (Exception e)
                {
                    Echo($"Exception: {e}\n---");
                    throw;
                }
                _tickMultiplier = 0;
            } 
        }

        private void RunCommand(string argument)
        {

        }

        private void RunContinuousLogic()
        {
            if(_CheckBlock.IsFunctional && _CheckBlock.HasPlayerAccess(Me.OwnerId))
            {
                Echo($"Master: {_CheckBlock.CustomName}\nWorking\n");
            } else
            {
                GridTerminalSystem.GetBlocksOfType(_Warheads);
                Echo($"Master: {_CheckBlock.CustomName}\nBroken\nWarheads: {_Warheads.Count}\n");

                if (!countdown)
                {
                    countdown = true;
                    foreach (IMyWarhead warhead in _Warheads)
                    {
                        warhead.IsArmed = true;
                        warhead.DetonationTime = TIME;
                        warhead.StartCountdown();

                    }
                    Runtime.UpdateFrequency = UpdateFrequency.None;
                    _LogOutput?.WriteText("\nRUN!!!", true);
                }
            }
        }

        private void EchoToLCD(string text)
        {
            text = "Boom LCD\n" + text;
            _LogOutput?.WriteText(text, false);
        }
    }
}
