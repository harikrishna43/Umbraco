﻿using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace Umbraco.Web.Trees
{
    [CollectionDataContract(Name = "nodes", Namespace = "")]
    public class TreeNodeCollection : List<TreeNode>
    {        
    }
}