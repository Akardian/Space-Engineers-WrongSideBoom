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
        // The Warhead detonation tine
        private int TIME = 20;

        // The LCD debug screen
        private const string LCD = "Boom LCD";

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

                _LogOutput?.WriteText("", true);
                Echo("Boom LCD\n");

                GridTerminalSystem.GetBlocksOfType(_Warheads);

                if(TIME < 1 || TIME > 3600)
                {
                    throw new InvalidOperationException("_DetonationTime is out of bounds");
                }
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
        }

        private void RunCommand(string argument)
        {
            _CheckBlock = null;
            _CheckBlock = GridTerminalSystem.GetBlockWithName(argument);
            if(_CheckBlock != null)
            {
                Echo(_CheckBlock.CustomName);
                Runtime.UpdateFrequency = UpdateFrequency.Update100;
            } else
            {
                Echo($"Block not {argument} found");
            }                
        }

        private void RunContinuousLogic()
        {
            if(_CheckBlock.IsFunctional && _CheckBlock.HasPlayerAccess(Me.OwnerId))
            {
                Echo($"CheckBlock: {_CheckBlock.CustomName}\nWorking");
            } else
            {
                GridTerminalSystem.GetBlocksOfType(_Warheads);
                Echo($"CheckBlock: {_CheckBlock.CustomName}\nBroken\nWarheads: {_Warheads.Count}");

                if (!countdown)
                {
                    countdown = true;
                    foreach (IMyWarhead warhead in _Warheads)
                    {
                        warhead.IsArmed = true;
                        warhead.DetonationTime = TIME;
                        warhead.StartCountdown();
                    }
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
