using System;
using System.Collections.Generic;
using System.Linq;

namespace AdminStore.Models.Workflow
{
    public class WorkflowGraph
    {
        private const string NoInitialStateError = "Workflow does not have an initial state.";
        private const string StateHasNoNameError = "Workflow state is missing a name.";
        private const string TransitionHasNoFromStateError = "Workflow transition is missing a 'FromState' value.";
        private const string TransitionHasNoToStateError = "Workflow transition is missing a 'ToState' value.";

        private readonly WorkflowGraphNode _initialNode;
        private readonly Dictionary<string, WorkflowGraphNode> _nodesByName;

        public WorkflowGraph(IeWorkflow workflow)
        {
            if (workflow == null)
                throw new ArgumentNullException(nameof(workflow));

            if (workflow.States == null)
                throw new ArgumentNullException(nameof(workflow.States));

            if (workflow.TransitionEvents == null)
                throw new ArgumentNullException(nameof(workflow.TransitionEvents));

            _nodesByName = new Dictionary<string, WorkflowGraphNode>();

            foreach (var state in workflow.States)
            {
                if (string.IsNullOrWhiteSpace(state.Name))
                {
                    throw new ArgumentException(StateHasNoNameError);
                }

                var node = new WorkflowGraphNode(state);
                _nodesByName.Add(state.Name, node);

                if (node.IsInitial)
                {
                    _initialNode = node;
                }
            }

            if (_initialNode == null)
                throw new ArgumentException(NoInitialStateError);

            foreach (var transition in workflow.TransitionEvents)
            {
                if (string.IsNullOrWhiteSpace(transition.FromState))
                    throw new ArgumentException(TransitionHasNoFromStateError);

                if (string.IsNullOrWhiteSpace(transition.ToState))
                    throw new ArgumentException(TransitionHasNoToStateError);

                WorkflowGraphNode fromNode;
                WorkflowGraphNode toNode;

                if (_nodesByName.TryGetValue(transition.FromState, out fromNode) &&
                    _nodesByName.TryGetValue(transition.ToState, out toNode))
                {
                    fromNode.AddNextNode(toNode);
                }
            }
        }

        public bool IsConnected()
        {
            var visitedNodes = new HashSet<WorkflowGraphNode>();
            TraverseDiagram(_initialNode, visitedNodes);

            return visitedNodes.Count == _nodesByName.Count;
        }

        private static void TraverseDiagram(WorkflowGraphNode currentNode, ISet<WorkflowGraphNode> visitedNodes)
        {
            if (visitedNodes.Contains(currentNode))
            {
                return;
            }

            visitedNodes.Add(currentNode);

            foreach (var nextNode in currentNode.NextNodes.Where(node => !visitedNodes.Contains(node)))
            {
                TraverseDiagram(nextNode, visitedNodes);
            }
        }

        private class WorkflowGraphNode
        {
            private readonly IeState _state;
            private readonly List<WorkflowGraphNode> _nextNodes;

            public WorkflowGraphNode(IeState state)
            {
                _state = state;
                _nextNodes = new List<WorkflowGraphNode>();
            }

            public bool IsInitial => _state.IsInitial.HasValue && _state.IsInitial.Value;
            public IEnumerable<WorkflowGraphNode> NextNodes => _nextNodes.AsEnumerable();

            public void AddNextNode(WorkflowGraphNode node)
            {
                _nextNodes.Add(node);
            }
        }
    }
}
