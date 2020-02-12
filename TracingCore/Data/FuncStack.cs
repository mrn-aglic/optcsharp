using System.Collections.Immutable;
using System.Linq;

namespace TracingCore.Data
{
    public class FuncStack
    {
        public string FuncName { get; }
        public int FrameId { get; }
        public bool IsHighlighted { get; }
        public ImmutableList<OptVariableData> EncodedLocals { get; }
        public string UniqueHash => $"{FuncName}_f{FrameId}";
        public ImmutableList<string> OrderedVarNames { get; }

        // DO NOT UNDERSTAND PURPOSE YET
        public bool IsParent { get; }
        public bool IsZombie { get; }
        public ImmutableList<int> ParentFrameIdList { get; }

        public FuncStack(string funcName, int frameId, bool isHighlighted, ImmutableList<OptVariableData> encodedLocals)
        {
            FuncName = funcName;
            FrameId = frameId;
            IsHighlighted = isHighlighted;
            EncodedLocals = encodedLocals;

            ParentFrameIdList = ImmutableList<int>.Empty;
            OrderedVarNames = encodedLocals.Select(x => x.Name).ToImmutableList();
        }

        public FuncStack(string funcName, 
            int frameId, 
            bool isHighlighted, 
            ImmutableList<OptVariableData> encodedLocals,
            ImmutableList<string> orderedVarNames)
        {
            FuncName = funcName;
            FrameId = frameId;
            IsHighlighted = isHighlighted;
            EncodedLocals = encodedLocals;

            ParentFrameIdList = ImmutableList<int>.Empty;
            OrderedVarNames = orderedVarNames;
        }

        public FuncStack AddAndUpdate(ImmutableList<OptVariableData> encodedLocals)
        {
            var encodedLocalNames = encodedLocals.Select(x => x.Name);
            
            var alreadyInLocals = EncodedLocals.Where(x => encodedLocalNames.Contains(x.Name)).ToList();
            var remainingLocals = EncodedLocals.RemoveRange(alreadyInLocals);
            
            var orderedVarNames = OrderedVarNames.RemoveRange(alreadyInLocals.Select(x => x.Name)).AddRange(encodedLocalNames);
            var encLocals = encodedLocals.AddRange(remainingLocals);

            return new FuncStack(
                FuncName,
                FrameId,
                IsHighlighted,
                encLocals,
                orderedVarNames
            );
        }

        public FuncStack AddAndUpdate(OptVariableData encodedLocals)
        {
            return AddAndUpdate(ImmutableList<OptVariableData>.Empty.Add(encodedLocals));
        }

        public string GetNameWithLine(PyTutorStep pyTutorStep)
        {
            return $"{FuncName}:{pyTutorStep.Line}";
        }
    }
}