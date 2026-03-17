namespace Alpha
{
    public class AlphaRoot { }

    public class AlphaContainer
    {
        // Nested public class inside a public class
        public class NestedInAlpha { }
    }
}

namespace Alpha.Sub
{
    public class AlphaSubClass { }
}

namespace Beta
{
    public class BetaClass { }

    // Private class — should be excluded from analysis
    class PrivateBetaClass { }
}
