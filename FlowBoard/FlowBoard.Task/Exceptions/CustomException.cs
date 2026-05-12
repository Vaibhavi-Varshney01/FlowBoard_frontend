namespace FlowBoard.Card.Exceptions
{
    public class CardNotFoundException : Exception
    {
        public CardNotFoundException(int cardId)
            : base($"Card {cardId} not found.") { }
    }

    public class CardArchivedException : Exception
    {
        public CardArchivedException(int cardId)
            : base($"Card {cardId} is archived and cannot be modified.") { }
    }

    public class CardNotArchivedException : Exception
    {
        public CardNotArchivedException(int cardId)
            : base($"Card {cardId} is not archived.") { }
    }

    public class InvalidCardPositionException : Exception
    {
        public InvalidCardPositionException(string message)
            : base(message) { }
    }

    public class CardReorderMismatchException : Exception
    {
        public CardReorderMismatchException(int payload, int actual)
            : base($"Reorder payload has {payload} entries but list has {actual} active cards.") { }
    }
}