using System;
using System.Linq;
using System.Reflection;
using Godot;
using Godot.Collections;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
class SubnodeAttribute : System.Attribute
{
	public string NodePath { get; }

	public SubnodeAttribute(string nodePath = null)
	{
		NodePath = nodePath;
	}
}

public static class NodeExtensions
{
    public static Array<T> FindAllNodesOf<T>(this Godot.Node node) where T : Godot.Node
    {
		var matching = new Array<T>();
        foreach (Node child in node.GetChildren())
        {
            if (child is T tChild)
            {
                matching.Add(tChild);
            }

			foreach (T kid in child.FindAllNodesOf<T>())
			{
				matching.Add(kid);
			}
        }

        return matching;
    }

	public static T FindFirstNodeOf<T>(this Godot.Node node) where T : Godot.Node
	{
		return node.GetChildren().OfType<T>().FirstOrDefault();
	}

	public static void FindSubnodes(this Godot.Node node)
	{
		foreach (PropertyInfo prop in node.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            var subnodeAttribute = (SubnodeAttribute)Attribute.GetCustomAttribute(prop, typeof(SubnodeAttribute));
			if (subnodeAttribute != null)
			{
				string nodePath = subnodeAttribute.NodePath ?? prop.Name;
				Node subnode = node.GetNode(nodePath);
				prop.SetValue(node, subnode);
			}
        }

		foreach (FieldInfo field in node.GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
		{
			var Subnode = (SubnodeAttribute)Attribute.GetCustomAttribute(field, typeof(SubnodeAttribute));
			if (Subnode != null)
			{
				string nodePath = Subnode.NodePath ?? field.Name;
				Node subnode = node.GetNode(nodePath);
				field.SetValue(node, subnode);
			}
		}
	}
}
