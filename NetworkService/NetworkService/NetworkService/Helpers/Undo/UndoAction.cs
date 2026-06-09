using System;

namespace NetworkService.Helpers.Undo
{
    public class UndoAction
    {
        public string Description { get; private set; }

        private readonly Action execute;

        public UndoAction(string description, Action execute)
        {
            Description = description;
            this.execute = execute;
        }

        public void Execute()
        {
            if (execute != null)
            {
                execute();
            }
        }
    }
}