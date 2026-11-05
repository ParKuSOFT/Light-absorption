using Xunit; 
using Light_absorption.Core.Models; 
using Light_absorption.Core.Solvers; 
using System;

namespace Light_absorption.Tests
{
    public class SolverTests
    {
        [Fact]
        public void Test_StateAtZ0_IsOne() { var state = new ExperimentState(new double[] { 1.0, 1.0, 1.0 }); Assert.Equal(1.0, state.Values[0]); Assert.Equal(1.0, state.Values[1]); Assert.Equal(1.0, state.Values[2]); }
        [Fact]
        public void Test_StateAtLargeZ_TendsToZero()
        {
            var state = new ExperimentState(new double[] { 1.0, 1.0, 1.0 });
            var solver = new EulerSolver();
            var history = solver.Solve(state, maxZ: 50.0, dz: 0.1);

            var lastState = history[history.Count - 1];

            Assert.True(lastState.Values[0] < 0.01);
            Assert.True(lastState.Values[1] < 0.01);
            Assert.True(lastState.Values[2] < 0.01);
        }

        [Fact]
        public void Test_IntensityOrder_R_LessThan_G_LessThan_B()
        {
            var state = new ExperimentState(new double[] { 1.0, 1.0, 1.0 });
            var solver = new EulerSolver();
            var history = solver.Solve(state, maxZ: 2.0, dz: 0.1);

            var midState = history[10];

            Assert.True(midState.Values[0] < midState.Values[1]);
            Assert.True(midState.Values[1] < midState.Values[2]);
        }

        [Fact]
        public void Test_ExperimentState_Clone_IsIndependent()
        {
            var original = new ExperimentState(new double[] { 1.0, 1.0, 1.0 });
            var clone = original.Clone();

            clone.Values[0] = 0.5;

            Assert.NotEqual(original.Values[0], clone.Values[0]);
            Assert.Equal(1.0, original.Values[0]);
        }
    }
}