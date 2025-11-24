using System.Collections;
using UnityEngine;

public class InteractiveFlash : MonoBehaviour
{
    [SerializeField] public Color _flashColor = Color.white;
    [SerializeField] public Color _pushColor = Color.black;
    [SerializeField] private float _flashtime = 2.0f;

    private bool isFlashing = true;

    private SpriteRenderer _spriteRenderer;
    private Material _material;

    private Coroutine _flashCoroutine;
    private Coroutine _loopFlashCoroutine;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        Init();
    }

    private void Init()
    {
        // Material _material;

        _material = _spriteRenderer.material;

        StartLoopFlash(0.2f);

    }

    public void StartLoopFlash(float interval = 0.2f)
    {
        if (_loopFlashCoroutine != null)
            StopCoroutine(_loopFlashCoroutine);

        isFlashing = true;
        _loopFlashCoroutine = StartCoroutine(LoopFlasher(interval));
    }

    public void StopLoopFlash()
    {
        if (_loopFlashCoroutine != null)
        {
            isFlashing = false;
            StopCoroutine(_loopFlashCoroutine);
            _loopFlashCoroutine = null;
            SetFlashAmount(0);  // reset
        }
    }

    private IEnumerator LoopFlasher(float interval)
    {
        while (isFlashing)
        {
            yield return Flasher();                 // 完整閃一下
            yield return new WaitForSeconds(interval); // 等待間隔
        }
    }


    private IEnumerator Flasher()
    {
        // set color
        SetFlashColor(_flashColor);
        float ratio = 0f;
        float elapsedTime = 0f;
        float currentFlashamount = 0f;

        while(elapsedTime < _flashtime)
        {
            elapsedTime += Time.deltaTime;

            if(elapsedTime < _flashtime / 2)
            {
                ratio = elapsedTime / (_flashtime / 2);
            }
            else
            {
                ratio = (_flashtime - elapsedTime) / (_flashtime / 2);
            }

            currentFlashamount = Mathf.Lerp(0.5f, 0f, ratio);

            SetFlashAmount(currentFlashamount);


            yield return null;
        }
    }

    public void SetFlashColor(Color _color)
    {
        _material.SetColor("_FlashColor", _color);
    }

    public void SetFlashAmount(float amount)
    {
        _material.SetFloat("_FlashAmount", amount);
    }
}
