using UnityEngine;

public class DetailShaderGenerateStep : GenerateStep
{
    public override GameObject Process(GameObject gameObject)
    {
        var material = new Material(Shader.Find("Shader Graphs/AsteroidDetailShader"));
        var renderer = gameObject.GetComponent<Renderer>();

        renderer.material = material;

        return gameObject;
    }

    public override void AddGUI()
    {
        
    }
}
