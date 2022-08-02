using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Threading.Tasks;

[CustomEditor(typeof(PathCreator))]
public class PathCreatorEditor : Editor
{
    private List<Vector3> positions = new List<Vector3>();
    private List<Dot> dots = new List<Dot>();
    private Vector3 mousePosition;
    private int clear_path = -1;
    private bool _lock = false;
    private int select_move = -1;
    private bool move = false;
    private int dotIndis = 0;
    private Dot tempDot;
    private PathCreator creater;
    private bool drawable = true;

    private void OnEnable()
    {
        creater = (PathCreator)target;
    }

    private void OnSceneGUI()
    {
        if (!_lock && select_move == -1) DotsEngine();
        if (select_move == 1) Select_Move_Dots();
        ButtonsEngine();
        if (drawable) Draw();
    }

    private void Draw()
    {
        foreach (Dot dot in dots)
        {
            dot.Draw();
            Handles.DrawDottedLine(dot.position, dot.NextDot.position, 4f);
            Handles.DrawDottedLine(dot.position, dot.PreviousDot.position, 4f);
        }
    }

    private void Select_Move_Dots()
    {
        mousePosition = Event.current.mousePosition;

        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (!move)
            {
                foreach (Dot _dot in dots)
                {
                    if (Mathf.Pow((hit.point.x - _dot.position.x), 2) + Mathf.Pow((hit.point.z - _dot.position.z), 2) < Mathf.Pow(_dot.radius, 2f))
                    {
                        _dot.Highlight(Color.green);
                        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                        {
                            move = true;
                            tempDot = _dot;
                        }
                    }
                    else
                    {
                        _dot.UnHighlight();
                    }
                }
            }
            else
            {
                tempDot.position = hit.point;
                if (Event.current.type == EventType.MouseUp)
                {
                    move = false;
                }
            }
        }
    }

    private void DotsEngine()
    {
        mousePosition = Event.current.mousePosition;

        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                Dot dot = new Dot(Color.blue, 0.25f, hit.point, dotIndis);
                if (dotIndis == 0)
                {
                    dot.PreviousDot = dot;
                    dot.NextDot = dot;
                }
                else
                {
                    dot.PreviousDot = dots[dotIndis - 1];
                    dot.PreviousDot.NextDot = dot;
                    dot.NextDot = dot;
                }
                dots.Add(dot);
                positions.Add(dot.position);
                dotIndis++;
            }
        }
    }

    private List<Vector3> ChaikinCurvesEngine()
    {
        List<Vector3> points = new List<Vector3>();

        foreach (Dot d in dots)
        {
            points.Add(d.position);
        }

        if (creater.smoothiness == 0) creater.smoothiness = 1;
        for (int a = 0; a < creater.smoothiness; a++)
        {
            points = chaikin(points);
        }
        return points;
    }

    private List<Vector3> chaikin(List<Vector3> data)
    {
        Vector3 first = data[0];
        Vector3 last = data[data.Count - 1];

        List<Vector3> datas = new List<Vector3>();
        datas.Add(first);

        for (int i = 0; i < data.Count - 1; i++)
        {
            Vector3 p0 = data[i];
            Vector3 p1 = data[i + 1];

            float dx = p1.x - p0.x;
            float dy = p1.y - p0.y;
            float dz = p1.z - p0.z;

            Vector3 v1 = new Vector3(p0.x + dx * 0.25f, p0.y + dy * 0.25f, p0.z + dz * 0.25f);
            Vector3 v2 = new Vector3(p0.x + dx * (1 - 0.25f), p0.y + dy * (1 - 0.25f), p0.z + dz * (1 - 0.25f));

            datas.Add(v1);
            datas.Add(v2);
        }
        datas.Add(last);
        return datas;
    }

    private void ButtonsEngine()
    {
        Color tempColor = GUI.backgroundColor;

        Handles.BeginGUI();

        Rect rect = new Rect(10, 10, 100, 30);
        if (GUI.Button(rect, "Clear All Paths"))
        {
            dots.Clear();
            dotIndis = 0;
            creater.ClearAll();
            positions.Clear();
        }
        //------------------------------------//
        if (clear_path == 1)
        {
            GUI.backgroundColor = Color.blue;
        }
        else
        {
            GUI.backgroundColor = tempColor;
        }
        Rect rect2 = new Rect(10, 50, 130, 30);
        if (GUI.Button(rect2, "Clear Selected Path"))
        {
            clear_path *= -1;

            if (clear_path == 1)
            {
                _lock = true;
            }
            else
            {
                _lock = false;
            }
        }
        GUI.backgroundColor = tempColor;
        //------------------------------------//
        Rect rect3 = new Rect(10, 90, 100, 30);
        if (GUI.Button(rect3, "Generate Path"))
        {
            drawable = false;

            List<Vector3> vectorList = new List<Vector3>();
            vectorList = ChaikinCurvesEngine();

            dots.Clear();
            dotIndis = 0;

            foreach (Vector3 v in vectorList)
            {
                Dot dot = new Dot(Color.blue, 0.1f, v, dotIndis);

                if (dotIndis == 0)
                {
                    dot.PreviousDot = dot;
                    dot.NextDot = dot;
                }
                else
                {
                    dot.PreviousDot = dots[dotIndis - 1];
                    dot.PreviousDot.NextDot = dot;
                    dot.NextDot = dot;
                }
                dots.Add(dot);
                dotIndis++;
            }

            drawable = true;
        }
        //------------------------------------//
        if (select_move == 1)
        {
            GUI.backgroundColor = Color.blue;
        }
        else
        {
            GUI.backgroundColor = tempColor;
        }
        Rect rect4 = new Rect(150, 10, 120, 30);
        if (GUI.Button(rect4, "Select and Move"))
        {
            GUI.skin.button.onFocused.textColor = Color.blue;
            select_move *= -1;
        }
        GUI.backgroundColor = tempColor;
        //------------------------------------//
        Rect rect5 = new Rect(10, 130, 60, 30);
        if (GUI.Button(rect5, "Create"))
        {
            positions.Clear();

            foreach (Dot dot in dots)
            {
                positions.Add(dot.position);
            }

            creater.Generate(positions);
        }

        Handles.EndGUI();

        if (clear_path == 1)
        {
            ClearPath();
        }

    }

    private void ClearPath()
    {
        mousePosition = Event.current.mousePosition;

        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            foreach (Dot dot in dots)
            {
                if (Mathf.Pow((hit.point.x - dot.position.x), 2) + Mathf.Pow((hit.point.z - dot.position.z), 2) < Mathf.Pow(dot.radius, 2f))
                {
                    dot.Highlight(Color.red);
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        DeleteThatNode(dot);
                    }
                }
                else
                {
                    if (dot.IsHighlighted())
                    {
                        dot.UnHighlight();
                    }
                }
            }
        }
    }

    private void DeleteThatNode(Dot dot)
    {
        dot.PreviousDot.NextDot = dot.NextDot;
        dot.NextDot.PreviousDot = dot.PreviousDot;

        dots.Remove(dot);

        dotIndis--;
    }


    public class Dot : Editor
    {
        public Dot NextDot, PreviousDot;
        public bool IsHighlightedBool;
        public Color color;
        public float radius;
        public Vector3 position;
        public int indis;

        public Dot(Color color, float radius, Vector3 position, int indis)
        {
            this.color = color;
            this.radius = radius;
            this.position = position;
            this.indis = indis;
            this.IsHighlightedBool = false;
        }
        public void Draw()
        {
            Handles.color = this.color;
            Handles.DrawSolidDisc(position, Vector3.up, radius);
        }
        public void ChangeColor(Color color)
        {
            this.color = color;
        }
        public void Highlight(Color color)
        {
            ChangeColor(color);
            IsHighlightedBool = true;
        }
        public void UnHighlight()
        {
            IsHighlightedBool = false;
            ChangeColor(Color.blue);
        }
        public bool IsHighlighted()
        {
            return IsHighlightedBool;
        }
    }
}