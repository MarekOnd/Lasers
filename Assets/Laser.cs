using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public Vector3 startPosition;
    public Vector3 startDirection;

    LineRenderer lineRenderer;
    Vector3[] positions;
    Vector3 direction;

    public int iterations;
    public int stepSize;
    public int speed;

    //
    private float indexOfRefraction = 1;
    private GameObject insideObject = null;

    //visual
    public Material material;

    public AnimationCurve curve;
    

    // Start is called before the first frame update
    void Start()
    {
        
        direction = startDirection;
        positions = new Vector3[iterations];
        positions[0] = startPosition;

        this.gameObject.AddComponent<LineRenderer>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = iterations;
        lineRenderer.material = material;
        lineRenderer.generateLightingData = true;
        lineRenderer.widthCurve = curve;
        
        
    }

    // Update is called once per frame
    void Update()
    {
        CalculatePath();
        lineRenderer.SetPositions(positions);
    }

    void CalculatePath()
    {
        positions = new Vector3[iterations];
        positions[0] = startPosition;
        direction = startDirection;

        
        for (int i = 0; i < iterations - 1; i++)
        {
            //positions[i + 1] = positions[i] + direction * stepSize;
            Ray ray = new Ray(positions[i], direction.normalized);
            RaycastHit hit;
            bool enteredObject = false;
            //raycasting front
            if(Physics.Raycast(ray,out hit,stepSize))
            {
                positions[i + 1] = positions[i] + direction.normalized * hit.distance;
                GameObject gameObject = hit.collider.gameObject;
               
                if (gameObject.GetComponent<LightProperties>())
                {
                    if(gameObject.GetComponent<LightProperties>().reflective == true)
                    {
                        direction = direction - 2 * (Vector3.Dot(hit.normal,direction)) * hit.normal.normalized;
                    }
                    else if(gameObject.GetComponent<LightProperties>().refractive == true)
                    {
                        
                        UpdateIndex();
                        float sinFi = gameObject.GetComponent<LightProperties>().index / indexOfRefraction * Vector3.Dot(direction, hit.normal) / (direction.magnitude*hit.normal.magnitude);//uhel paprsku po odrazu
                        if(sinFi>1)//odrazi se
                        {
                            //direction = direction - 2 * (Vector3.Dot(hit.normal, direction)) * hit.normal.normalized;
                            direction = Vector3.Reflect(direction, hit.normal);
                        }
                        else//vejde dovnitr
                        {
                            direction = direction.normalized - hit.normal.normalized * gameObject.GetComponent<LightProperties>().index / indexOfRefraction;
                            insideObject = gameObject;
                            enteredObject = true;
                        }
                        
                    }
                }
                else
                {
                    direction = Vector3.zero;
                }
            }
            else
            {
                positions[i + 1] = positions[i] + direction.normalized * stepSize;
            }

            //raycasting backwards to see if it left a material
            if(insideObject != null && !enteredObject)//pokud to nevlezlo do jineho
            {
                ray = new Ray(positions[i +1], -direction.normalized);
                if (Physics.Raycast(ray, out hit, stepSize))//paprsek zpet
                {
                    if (insideObject == hit.collider.gameObject)//pokud to trefi objekt ve kterem ma byt tak se k nemu vrati a spocita jestli z neho vyleze
                    {
                        positions[i + 1] = hit.point;
                        UpdateIndex();
                        float sinFi = 1/indexOfRefraction * Vector3.Dot(direction, hit.normal) / (direction.magnitude * hit.normal.magnitude);//uhel paprsku po odrazu
                        if (sinFi > 1)//odrazi se zpet
                        {
                            //direction = direction - 2 * (Vector3.Dot(hit.normal, direction)) * (-hit.normal.normalized);
                            direction = Vector3.Reflect(direction, hit.normal);
                        }
                        else//vyjde ven
                        {
                            direction = Vector3.Dot(direction.normalized,indexOfRefraction*hit.normal.normalized) * direction.normalized +direction.normalized - indexOfRefraction * hit.normal.normalized;
                            Debug.Log(hit.normal.magnitude);
                            insideObject = null;
                        }
                    }

                }
            }

        }
    }

    void CheckCollision()
    {
        
    }

    void UpdateIndex()
    {
        if(insideObject)
        {
            indexOfRefraction = insideObject.GetComponent<LightProperties>().index;
        }
    }


}
