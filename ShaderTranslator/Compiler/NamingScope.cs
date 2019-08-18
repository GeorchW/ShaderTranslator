using System.Collections.Generic;

namespace ShaderTranslator
{
    class NamingScope
    {
        HashSet<string> takenNames = new HashSet<string>();
        NameManager nameManager;
        NamingScope? parent;

        public NamingScope(NamingScope parent)
        {
            this.parent = parent;
            this.nameManager = parent.nameManager;
        }

        public NamingScope(NameManager nameManager)
        {
            this.parent = null;
            this.nameManager = nameManager;
        }

        public string GetFreeName(string name)
        {
            string result = GetFreeNameInternal(name);
            takenNames.Add(result);
            return result;
        }

        private string GetFreeNameInternal(string name)
        {
            name = nameManager.Legalize(name);
            bool isAvailable(string name)
            {
                if (nameManager.IsKeyword(name)) return false;
                NamingScope? currentScope = this;
                do
                {
                    if (currentScope.takenNames.Contains(name))
                        return false;
                    currentScope = currentScope.parent;
                }
                while (currentScope != null);
                return true;
            }
            if (isAvailable(name)) return name;
            else
            {
                string newName;
                int i = 1;
                do
                {
                    newName = nameManager.Legalize($"{name}_{i}");
                    i++;
                } while (!isAvailable(newName));
                return newName;
            }
        }
    }
}
