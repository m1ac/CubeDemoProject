using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;

public class Block
{
    public GameObject BlockObject;
    public Block(GameObject obj)
    {
        BlockObject = obj;
    }
}

public class CreateGame : MonoBehaviour
{
    FileInfo f;
    int score = 0, best_score = 0;
    string powerup = "";
    bool gameOver = false;
    public Texture2D crs;
    GameObject b, hammer;
    public GameObject block;
    public GameObject[] hints = new GameObject[4];
    Block[,] blocks = new Block[5, 5];
    System.Random rnd;
    Queue PaletteColors;
    List<GameObject> PaletteObjects;
    List<GameObject> CheckObjects;
    List<Color> colors;

    int GetPaletteCount()
    {
        List<int> l = new List<int> { 1, 2, 3, 4, 2, 3, 2, 3 };
        return l[rnd.Next(0, l.Count)];
    }

    Color GetRandomColor()
    {
        List<Color> colors = new List<Color> { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta };
        int ind = rnd.Next(0, colors.Count);
        return colors[ind];
    }

    void GetPalette()
    {
        while (PaletteObjects.Count > 0)
        {
            Destroy(PaletteObjects[0]);
            PaletteObjects.RemoveAt(0);
        }
        int c = GetPaletteCount();
        for (int i = 0; i < c; i++)
        {
            Vector3 p = new Vector3(i * 0.6f, 7, 0);
            GameObject o = (GameObject)Instantiate(block, p, block.transform.rotation);
            Vector3 s = new Vector3(4, 4, 1);
            o.transform.localScale = s;
            PaletteObjects.Add(o);
            Color rc = GetRandomColor();
            o.GetComponent<Renderer>().material.color = rc;
            PaletteColors.Enqueue(rc);
        }
    }

    private void OnGUI()
    {
        GUIStyle g = new GUIStyle();
        GUIStyle go = new GUIStyle();
        g.fontSize = 45;
        go.fontSize = 140;
        GUI.Label(new Rect(5, 35, 200, 100), "Best Score: " + best_score.ToString(), g);
        GUI.Label(new Rect(5, 85, 200, 100), "Score: " + score.ToString(), g);
        if (gameOver)
        {
            GUI.Label(new Rect(5, Screen.height / 2, Screen.width, Screen.height), "Game Over!", go);
        }
    }

    bool IsGameOver()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (blocks[i, j].BlockObject.GetComponent<Renderer>().material.color == Color.white)
                {
                    return false;
                }
            }
        }
        return true;
    }

    void NearBlock(Color c1, int i, int j, int _i, int _j)
    {
        Color clr;
        try
        {
            clr = blocks[i + _i, j + _j].BlockObject.GetComponent<Renderer>().material.color;

            if (c1 == clr && CheckObjects.Contains(blocks[i + _i, j + _j].BlockObject) == false)
            {
                CheckObjects.Add(blocks[i + _i, j + _j].BlockObject);
            }
        }
        catch (Exception) { }
    }

    void Check(Color cc)
    {
        CheckObjects.Clear();
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                Color c1 = blocks[i, j].BlockObject.GetComponent<Renderer>().material.color;
                if (c1 == cc)
                {
                    NearBlock(c1, i, j, -1, 0);
                    NearBlock(c1, i, j, 0, -1);
                    NearBlock(c1, i, j, 1, 0);
                    NearBlock(c1, i, j, 0, 1);
                }
            }
        }

        if (CheckObjects.Count >= 3)
        {
            foreach (var item in CheckObjects)
            {
                score += 10;
                item.GetComponent<Renderer>().material.color = Color.white;
            }
        }
        CheckObjects.Clear();
    }

    void Start ()
    {
        PaletteColors = new Queue();
        rnd = new System.Random();
        PaletteObjects = new List<GameObject>();
        CheckObjects = new List<GameObject>();
        colors = new List<Color> { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.cyan };
        if (Application.platform == RuntimePlatform.Android)
        {
            f = new FileInfo(Application.persistentDataPath + "\\" + "CubeDemo.txt");
        }
        else
        {
            f = new FileInfo(Application.dataPath + "\\" + "CubeDemo.txt");
        }

        foreach (var item in hints)
        {
            item.SetActive(false);
        }   
        
        GetPalette();
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                Vector3 p = new Vector3(j, i, 0);
                GameObject o = (GameObject)Instantiate(block, p, block.transform.rotation);
                o.GetComponent<Renderer>().material.color = Color.white;
                o.name = j + "-" + i;
                blocks[j, i] = new Block(o);
            }
        }

        if (f.Exists)
        {
            StreamReader r = File.OpenText(f.FullName);
            best_score = int.Parse(r.ReadLine());
            score = int.Parse(r.ReadLine());
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    string[] rgb = r.ReadLine().Split(',');
                    Color c = new Color(float.Parse(rgb[0]), float.Parse(rgb[1]), float.Parse(rgb[2]));
                    blocks[i, j].BlockObject.GetComponent<Renderer>().material.color = c;
                }
            }
            r.Close();
        }
    }
    
    void Update ()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, 1000);
        if (hit)
        {
            b = hit.collider.gameObject;
            if (Input.GetMouseButton(0))
            {
                if (b.name == "btn_hammer")
                {
                    b.transform.localScale = new Vector3(0.5f, 0.5f);
                    hammer = b;
                    powerup = "hammer";
                    //Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                }
                else if (b.name == "btn_hint")
                {
                    foreach (var item in hints)
                    {
                        item.GetComponent<Renderer>().material.color = colors[rnd.Next(0, colors.Count)];
                        item.SetActive(true);
                    }
                    powerup = "";
                }
                else if (b.name == "btn_renew")
                {
                    while (PaletteColors.Count > 0)
                    {
                        PaletteColors.Dequeue();
                    }
                    GetPalette();
                    powerup = "";
                }
                else
                {
                    if (powerup == "hammer")
                    {
                        b.GetComponent<Renderer>().material.color = Color.white;
                        powerup = "";
                        hammer.transform.localScale = new Vector3(0.3f, 0.3f);
                        Thread.Sleep(500);
                    }
                    else if (powerup == "hint")
                    {

                    }
                    else if (powerup == "renew")
                    {
                        
                    }
                    else if (b.GetComponent<Renderer>().material.color == Color.white && PaletteColors.Count > 0 && powerup == "")
                    {
                        foreach (var item in hints)
                        {
                            item.SetActive(false);
                        }
                        b.GetComponent<Renderer>().material.color = (Color)PaletteColors.Dequeue();
                        Destroy(PaletteObjects[0]);
                        PaletteObjects.RemoveAt(0);
                    }
                }

            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (hints[0].GetComponent<Renderer>().material.color != Color.white && PaletteObjects.Count==0)
            {
                for (int i = 0; i < hints.Length; i++)
                {
                    Vector3 p = new Vector3(i * 0.6f, 7, 0);
                    GameObject o = (GameObject)Instantiate(block, p, block.transform.rotation);
                    Vector3 s = new Vector3(4, 4, 1);
                    o.transform.localScale = s;
                    PaletteObjects.Add(o);
                    Color rc = hints[i].GetComponent<Renderer>().material.color;
                    hints[i].GetComponent<Renderer>().material.color = Color.white;
                    o.GetComponent<Renderer>().material.color = rc;
                    PaletteColors.Enqueue(rc);
                }
            }
            else
            {
                if (PaletteColors.Count == 0)
                {
                    foreach (var item in colors)
                    {
                        Check(item);
                    }
                    GetPalette();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (f.Exists)
            {
                StreamReader r = File.OpenText(f.FullName);
                best_score = int.Parse(r.ReadLine());
                if (score > best_score)
                {
                    best_score = score;
                }
                r.Close();
            }
            else
            {
                best_score = score;
            }
            StreamWriter w = f.CreateText();
            w.WriteLine(best_score);
            w.WriteLine(score);
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Color c = blocks[i, j].BlockObject.GetComponent<Renderer>().material.color;
                    w.WriteLine(c.r + "," + c.g + "," + c.b);
                }
            }
            w.Close();
            Application.Quit();
        }

        if (UnityEngine.Object.FindObjectsOfType<GameObject>().Length > 1)
        {
            if (IsGameOver())
            {
                gameOver = true;
                foreach (GameObject o in UnityEngine.Object.FindObjectsOfType<GameObject>())
                {
                    if (o.tag != "MainCamera")
                    {
                        Destroy(o);
                    }
                }
            }
            
        }
    }
}
