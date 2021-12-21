using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class AsteroidTool : EditorWindow
{
    private int NR_FOLD = 4, CHILD_OBJECT = 0, MESH_SETTINGS = 1, CRATER_SETTINGS = 2, PHYSICS_SETTINGS = 3;

    //The asteroid object used in the unity editor
    GameObject asteroid;

    //All the information about the asteroid and where we store the data
    AsteroidData asteroidData;

    //Boolean list that keeps track if the child window is folded out or in
    bool[] foldList;

    private List<GenerateStep> steps;

    //Boolean that keeps track if the users wants to see the documentation in the editor
    private bool infoToggled;
    private Vector2 scrollPosition = Vector2.zero;

    //Add menu item named "Asteroid Tool" to the Window menu
    [MenuItem("Window/Asteroid Tool")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(AsteroidTool));
    }

    //When the Asteroid Tool window is opened initialize all the generation steps and get the selected object 
    public void Awake()
    {
        steps = new List<GenerateStep>();
        steps.Add(new ShrinkWrapMeshGenerateStep());
        steps.Add(new SmoothMeshGenerateStep());
        steps.Add(new DetailShaderGenerateStep());
        steps.Add(new DetailGenerateStep());
        steps.Add(new PhysicsGenerateStep());
        
        SetEditorSelectedObject();

        //If there already exists an asteroid object get the needed script from the game object
        if (asteroid != null)
        {
            asteroidData = asteroid.GetComponent<AsteroidData>();
        }

        foldList = new bool[NR_FOLD];
    }

    //Update with method to refresh the selection of the user in the editor
    private void Update()
    {
        SetEditorSelectedObject();
    }

    //Here are all the fields shown and update
    private void OnGUI()
    {
        //Toggle for if the documentation is shown or not
        infoToggled = GUILayout.Toggle(infoToggled, "See value info");

        if (GUILayout.Button("Generate Asteroid"))
        {
            GenerateAsteroidObject();
        }
        ShowDocumentation("This button generates one asteroid object. This is used as center point and basis for the whole asteroid.");

        //Create scroll view because if the documentation is shown the last fields do not fit in the editor
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, true, true, GUILayout.Width(this.position.width), GUILayout.Height(this.position.height - 100));

        //Check if asteroid is null if it is then set placeholders
        if (asteroid != null)
        {
            //Fold menu for child menu
            foldList[CHILD_OBJECT] = EditorGUILayout.Foldout(foldList[CHILD_OBJECT], "Add Object");

            if (foldList[CHILD_OBJECT])
            {
                SpawnChildObject();
            }

            //Fold menu for mesh menu
            foldList[MESH_SETTINGS] = EditorGUILayout.Foldout(foldList[MESH_SETTINGS], "Mesh settings");

            if (foldList[MESH_SETTINGS])
            {
                MeshSettings();
            }

            //Fold menu for crater settings 
            foldList[CRATER_SETTINGS] = EditorGUILayout.Foldout(foldList[CRATER_SETTINGS], "Crater settings");

            if (foldList[CRATER_SETTINGS])
            {
                CraterSettings();
            }
            
            //Fold menu for crater settings 
            foldList[PHYSICS_SETTINGS] = EditorGUILayout.Foldout(foldList[PHYSICS_SETTINGS], "Physics settings");

            if (foldList[PHYSICS_SETTINGS])
            {
                PhysicsSettings();
            }
        }
        else
        {
            SetPlaceholders();
        }
        EditorGUILayout.EndScrollView();
    }

    //This method creates a asteroid game object and attaches all the necessary components like a collider, mesh and other scripts. It also adds a sphere as the first child as default primitive shape for the asteroid.
    private void GenerateAsteroidObject()
    {
        asteroid = new GameObject("Asteroid");
        asteroid.transform.position = new Vector3(0, 0, 0);
        asteroid.AddComponent<MeshFilter>();
        asteroid.AddComponent<MeshRenderer>();
        asteroid.AddComponent<MeshCollider>();
        asteroidData = asteroid.AddComponent<AsteroidData>();

        //Create child object and set the asteroid as its parent
        GameObject child = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        child.transform.localScale = new Vector3(100, 100, 100);
        child.transform.parent = asteroid.transform;
    }

    //GUI method that shows all the buttons that can add a primitive game object. If the button is pressed the method in the if statement is called
    private void SpawnChildObject()
    {
        if (GUILayout.Button("Add Cube"))
        {
            CreateChildObject(PrimitiveType.Cube);
        }

        if (GUILayout.Button("Add Sphere"))
        {
            CreateChildObject(PrimitiveType.Sphere);
        }

        if (GUILayout.Button("Add Cylinder"))
        {
            CreateChildObject(PrimitiveType.Cylinder);
        }
        if (GUILayout.Button("Add Capsule"))
        {
            CreateChildObject(PrimitiveType.Capsule);
        }
        ShowDocumentation("These buttons add primitive game objects to the main asteroid object. [cube, sphere, cylinder, capsule] \nThese primitive objects are the basis of the asteroid shape when creating it.");
    }

    //Method for creating a child object for the asteroid and sets it back on z axis
    private void CreateChildObject(PrimitiveType type)
    {
        if (asteroid != null)
        {
            GameObject child = GameObject.CreatePrimitive(type);

            child.transform.localScale = new Vector3(100, 100, 100);

            //Check if asteroid already has a child so the next child is placed properly 
            if (asteroid.transform.childCount > 0)
            {
                Transform lastChild = asteroid.transform.GetChild(asteroid.transform.childCount - 1);
                child.transform.position = lastChild.position + Vector3.back * 100;
            }
            else
            {
                child.transform.position = asteroid.transform.position;
            }

            child.transform.parent = asteroid.transform;
        }
        else
        {
            //If the asteroid object is empty show a dialog option that there must be one selected.
            EditorUtility.DisplayDialog("No asteroid selected in the editor", "Please select an asteroid first", "Cancel", "Ok");
        }
    }

    //GUI method to show all the mesh settings in the mesh settings fold menu
    private void MeshSettings()
    {
        asteroidData!.subDivideRecursions = EditorGUILayout.IntSlider("Subdivide Recursions", asteroidData!.subDivideRecursions, 1, 6);
        ShowDocumentation("The subdivide recursions change the level of detail the base mesh of the asteroid will become. " +
            "Every recursion multiplies the amount of polygons by four, adding more detail. " +
            "Warning: Higher recursion levels will also impact the time needed for generation significantly, " +
            "values above five recursion may take exponentially more time. " +
            "\n[3-5 Recursions recommended] [1 recursion only works with specific settings]");

        asteroidData!.smoothRecursions = EditorGUILayout.IntSlider("Smoothing Recursions", asteroidData!.smoothRecursions, 1, 200);
        ShowDocumentation("The smoothing recursion controls the roundness of the asteroid. " +
            "Every recursion will place vertices closer together. " +
            "The leftover shapes will have a realistic asteroid volume. " +
            "Warning: The mesh will become smaller with every recursion, " +
            "high values can shrink the asteroid until it is gone. " +
            "\n[25-50 recursions recommended]");

        asteroidData!.indexFormat = (IndexFormat)EditorGUILayout.EnumPopup(asteroidData!.indexFormat);
        ShowDocumentation("Sets the level of detail on the mesh"
                        +"\n[U Int 32 recommended]");

        //If the button is pressed create the mesh and smoothing an add the shaders and texture
        if (GUILayout.Button("Generate Mesh"))
        {
            // Ask if the user is sure the want to generate the asteroid if the click the yes button then the asteroid can be generated
            if (!EditorUtility.DisplayDialog("Warning", "Generating the asteroid may take some time. Are you sure you want to proceed?", "Cancel", "Ok"))
            {
                steps[0].Process(asteroid);
                steps[1].Process(asteroid);
                steps[2].Process(asteroid);
            }
        }
    }

    //GUI method that shows all the crater settings in the crater fold menu
    private void CraterSettings()
    {
        asteroidData!.maxCraterSize = EditorGUILayout.Slider("Max crater size", asteroidData!.maxCraterSize, 1, 20);
        ShowDocumentation("The maximum crater size is used as maximum value for the width of crater generation. " +
            "The size is measured on local scale. \n[1 - 20 recommended]");

        asteroidData!.minCraterSize = EditorGUILayout.Slider("Min crater size", asteroidData!.minCraterSize, 0.1f, 5);
        ShowDocumentation("The minimum crater size is used as minimum value for the width of crater generation. " +
            "The size is measured on local scale. \n[0.1 - 5 recommended]");

        asteroidData!.CraterDepth = EditorGUILayout.Slider("Depth of crater", asteroidData!.CraterDepth, 0.1f, 10);
        ShowDocumentation("The depth of the crater is a value that sets how deep a crater is created. " +
            "This is based on the size of the original crater's width. \n[0.2 - 0.7 recommended]");

        asteroidData!.CraterAmount = EditorGUILayout.IntField("Amount of craters", asteroidData!.CraterAmount);
        ShowDocumentation("The amount of craters is a value that sets the amount of craters to the tool will generate. \n[100 - 150 recommended]");

        if(asteroidData!.maxCraterSize<=asteroidData!.minCraterSize)
        {
                asteroidData!.maxCraterSize = asteroidData!.minCraterSize + 1f;
        }
        
        // EditorGUILayout.EndToggleGroup();

        if (GUILayout.Button("Create craters"))
        {
            steps[3].Process(asteroid);
        }
    }

    private void PhysicsSettings()
    {
        asteroidData!.addGravity = EditorGUILayout.BeginToggleGroup("Add gravity", asteroidData!.addGravity);
        ShowDocumentation("The collision checkbox is checked if the asteroid should attract other objects.");

        asteroidData!.asteroidDensity = EditorGUILayout.Slider("Asteroid Density", asteroidData!.asteroidDensity, 0, 100);
        ShowDocumentation("The density is used to calculate the mass and the gravity pull strength indirectly. " +
                          "The higher the value, the higher the gravitational pull. \n[0.01 - 1 recommended]");
        
        EditorGUILayout.EndToggleGroup();

        asteroidData!.addColisions = EditorGUILayout.BeginToggleGroup("Add collisions", asteroidData!.addColisions);
        ShowDocumentation("The collision checkbox is checked if the asteroid should interact with comets. " +
                          "Interaction with comets will leave new craters on the asteroid.");
        
        asteroidData!.minForceRequired = EditorGUILayout.Slider("Min force required", asteroidData!.minForceRequired, 0.1f, 200);
        ShowDocumentation("The minimal force required is a value that is used as baseline to create new craters by collisions. " +
                          "This is calculated by overcoming the magnitude of impact, which is 'velocity * mass'. " +
                          "\n[1 - 5 is recommended (This should be set at your own discretion)]");

        asteroidData!.impactForceMultiplier = EditorGUILayout.Slider("Force Multiplier", asteroidData!.impactForceMultiplier, 0.1f, 10);
        ShowDocumentation("The amount with which the force is multiplied to create a bigger impact force with an object");

        EditorGUILayout.EndToggleGroup();
        
        if (GUILayout.Button("Add physics"))
        {
            steps[4].Process(asteroid);
        }
    }

    //GUI method that shows all the fold menus folded in and grayed out. This is to prevent null pointers if there is no asteroid game object set in the script
    public void SetPlaceholders()
    {
        EditorGUI.BeginDisabledGroup(asteroidData == null);

        EditorGUILayout.Foldout(foldList[PHYSICS_SETTINGS], "Add object");
        EditorGUILayout.Foldout(foldList[PHYSICS_SETTINGS], "Mesh settings");
        EditorGUILayout.Foldout(foldList[PHYSICS_SETTINGS], "Crater settings");
        EditorGUILayout.Foldout(foldList[PHYSICS_SETTINGS], "Additional settings");

        EditorGUI.EndDisabledGroup();

        //Disable all fold menu's
        foldList = new bool[NR_FOLD];
    }


    //Method that checks if the selected object in the unity editor of the parent of the object is an asteroid and if it is set all the values so we can edit that asteroid with the custom editor
    private void SetEditorSelectedObject()
    {
        GameObject selectedObject = Selection.activeGameObject;

        //Check if an object is selected in the editor
        if (selectedObject != null)
        {
            if (selectedObject.name == "Asteroid")
            {
                asteroid = selectedObject;
                asteroidData = asteroid.GetComponent<AsteroidData>();
            }
            //Check if the selected game object has a parent
            else if (selectedObject.transform.parent != null)
            {
                //If the select object has a parent check if that object is an asteroid
                if (selectedObject.transform.parent.name == "Asteroid")
                {
                    asteroid = selectedObject.transform.parent.gameObject;
                    asteroidData = asteroid.GetComponent<AsteroidData>();
                }
            }
        }
    }

    //Method that shows a help box with useful information about a field
    private void ShowDocumentation(string message)
    {
        if (infoToggled)
        {
            EditorGUILayout.HelpBox(message, MessageType.Info, true);
        }
    }

    //Method that updates the editor when there are changes made
    public void OnInspectorUpdate()
    {
        this.Repaint();
    }
}
