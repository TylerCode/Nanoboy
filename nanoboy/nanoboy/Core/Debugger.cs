﻿using System;
using System.Collections.Generic;

namespace nanoboy.Core
{
    /// <summary>
    /// Stores information about breakpoints.
    /// </summary>
    public sealed class Breakpoint
    {
        public enum BreakpointType
        {
            Execution,
            Memory,
            MemoryRead,
            MemoryWrite
        }
        public BreakpointType Type { get; set; }
        public int Offset { get; set; }

        public Breakpoint(BreakpointType type, int offset)
        {
            Type = type;
            Offset = offset;
        }
    }

    /// <summary>
    /// Observes the gameboy's processor and informs about any hitten breakpoints.
    /// </summary>
    public sealed class Debugger : IObserver<CPUStatusUpdate>
    {
        public sealed class BreakpointEventArgs : EventArgs
        {
            public CPUStatusUpdate Status { get; set; }

            public BreakpointEventArgs(CPUStatusUpdate status)
            {
                Status = status;
            }
        }
        public delegate void BreakpointEventHandler(object sender, BreakpointEventArgs e);
        public event BreakpointEventHandler Breakpoint;

        public List<Breakpoint> Breakpoints { get; set; }

        public Debugger()
        {
            Breakpoints = new List<Breakpoint>();
        }

        public void OnCompleted() { throw new NotImplementedException(); }
        public void OnError(Exception error) { throw new NotImplementedException(); }

        public void OnNext(CPUStatusUpdate value)
        {
            foreach (Breakpoint breakpoint in Breakpoints) {
                bool typematch = (breakpoint.Type == nanoboy.Core.Breakpoint.BreakpointType.Execution && value.Reason == CPUStatusUpdate.UpdateReason.Execution) |
                                 (breakpoint.Type == nanoboy.Core.Breakpoint.BreakpointType.Memory && (value.Reason == CPUStatusUpdate.UpdateReason.MemoryRead || value.Reason == CPUStatusUpdate.UpdateReason.MemoryWrite)) |
                                 (breakpoint.Type == nanoboy.Core.Breakpoint.BreakpointType.MemoryRead && value.Reason == CPUStatusUpdate.UpdateReason.MemoryRead) |
                                 (breakpoint.Type == nanoboy.Core.Breakpoint.BreakpointType.MemoryWrite && value.Reason == CPUStatusUpdate.UpdateReason.MemoryWrite);
                if (breakpoint.Offset == value.Offset && typematch) {
                    value.CPU.Running = false;
                    if (Breakpoint != null) {
                        Breakpoint(this, new BreakpointEventArgs(value));
                    }
                }
            }
        }

        public void Resume(CPUStatusUpdate status) 
        {
            status.CPU.Running = true;
        }
    }
}
