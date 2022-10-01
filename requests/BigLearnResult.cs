namespace XHRTools.requests
{
    public abstract class BigLearnResult
    {
        public abstract int Code { get; }
        public abstract string Msg { get; }
        public abstract string Data { get; }

        public bool IsSuccess()
        {
            return Code == 0;
        }

        public override string ToString()
        {
            return $"{nameof(Code)}: {Code}, {nameof(Msg)}: {Msg}, {nameof(Data)}: {Data}";
        }
    }
}