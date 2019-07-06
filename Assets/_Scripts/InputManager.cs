using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WindowsInput;
using WindowsInput.Native;
public class InputManager : MonoBehaviour
{
    [SerializeField] Vector3 TargetVector = Vector3.up;
    [SerializeField] Color basicColor = Color.white, HighlightedColor = Color.red;
    [SerializeField, Range(0, 1000)] float ScaleGradient = 750, PosGradient = 250;
    [SerializeField, Range(0, 0.1f)] float basicRadius = 0.05f, additionalRadius = 0.025f;
    [SerializeField, Range(0, 0.5f)] float highlightedPos = 0.4f;
    [SerializeField] string TriggerInputName;
    public string inputText = "H";
    public List<Transform> Keys = new List<Transform>();
    List<TextMesh> texts;
    InputSimulator IS = new InputSimulator();
    void Start()
    {
        texts = Keys.Select(x => x.GetComponent<TextMesh>()).ToList();
    }

    void Update()
    {
        var t = Keys.Where(p => p.position.y > transform.position.y).OrderBy(x =>
            Vector3.Cross(TargetVector.normalized, x.position - transform.position).sqrMagnitude).First();
        var index = Keys.IndexOf(t);
        Keys.ForEach(x =>
        {
            if (x.position.y > transform.position.y)
                x.localScale = Vector3.one * (basicRadius +
                    additionalRadius * Mathf.Clamp01(Mathf.Abs(1 - ScaleGradient * Vector3.Cross(TargetVector.normalized, x.position - transform.position).sqrMagnitude)));
            else
                x.localScale = Vector3.one * basicRadius;
        });
        texts.ForEach(s =>
        {
            s.offsetZ = -highlightedPos * Mathf.Clamp01(Mathf.Abs(1 - PosGradient * Vector3.Cross(TargetVector.normalized, s.transform.position - transform.position).sqrMagnitude));
        });
        texts.ForEach(x => x.color = basicColor);
        texts[index].color = HighlightedColor;
        inputText = texts[index].text;
        VirtualKeyCode vk;
        bool IsSuccess = false;
        Debug.Log(Input.GetJoystickNames()[0]);
        if (Input.GetAxis(TriggerInputName) > 0.1f && (IsSuccess = System.Enum.TryParse("VK_" + inputText, out vk)))
        {
            Debug.Log(IsSuccess);
            IS.Keyboard.KeyDown(vk);
        }
    }
}