using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PredType.Utils
{
    public class Sequence
    {
        private readonly string text;
        private readonly int wordCount;

        public Sequence(string text, int wordCount)
        {
            this.text = text;
            this.wordCount = wordCount;
        }

        public string Text
        {
            get { return text; }            
        }

        public int WordCount
        {
            get { return wordCount; }            
        }        

        protected bool Equals(Sequence other)
        {
            return string.Equals(Text, other.Text);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Sequence) obj);
        }

        private bool hashCalculated = false;
        private int hash;
        

        public override int GetHashCode()
        {
            if (hashCalculated)
                return hash;

            hash = (text != null ? text.GetHashCode() : 0);
            hashCalculated = true;
            return hash;
        }
    }
}
