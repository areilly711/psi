﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace TurtleROSSample
{
    using System;
    using Microsoft.Psi;
    using Microsoft.Psi.Components;

    public class TurtleComponent : IFiniteSourceComponent
    {
        private readonly Pipeline pipeline;
        private readonly Turtle turtle;

        private Action onCompleted;
        private bool stopped;

        public TurtleComponent(Pipeline pipeline, Turtle turtle)
        {
            pipeline.RegisterPipelineStartHandler(this, this.OnPipelineStart);
            pipeline.RegisterPipelineStopHandler(this, this.OnPipelineStop);
            this.pipeline = pipeline;
            this.turtle = turtle;
            this.Velocity = pipeline.CreateReceiver<Tuple<float, float>>(this, (c, _) => this.turtle.Velocity(c.Item1, c.Item2), nameof(this.Velocity));
            this.PoseChanged = pipeline.CreateEmitter<Tuple<float, float, float>>(this, nameof(this.PoseChanged));
        }

        public Receiver<Tuple<float, float>> Velocity { get; private set; }

        public Emitter<Tuple<float, float, float>> PoseChanged { get; private set; }

        public void Initialize(Action onCompleted)
        {
            this.onCompleted = onCompleted;
        }

        public void OnPipelineStart()
        {
            this.turtle.Connect();
            this.turtle.PoseChanged += this.OnPoseChanged;
        }

        public void OnPipelineStop()
        {
            this.stopped = true;
            this.turtle.Disconnect();
            this.turtle.PoseChanged -= this.OnPoseChanged;
            this.onCompleted();
        }

        private void OnPoseChanged(object sender, Tuple<float, float, float> pose)
        {
            if (!this.stopped)
            {
                this.PoseChanged.Post(pose, this.pipeline.GetCurrentTime());
            }
        }
    }
}