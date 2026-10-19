using Tractor.Net;
using UnityEngine;

public class UITestCanvas : MonoBehaviour
{
    public static UITestCanvas Instance;

    [SerializeField] GameObject additionalMenuPC;
    [SerializeField] GameObject additionalMenuTablet;

    RouterPC routerPC;
    RouterTablet routerTablet;

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Cannot create UITestCanvas");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        Init();
    }

    public void Init()
    {
        routerPC = RouterPC.Instance;
        routerTablet = RouterTablet.Instance;
    }

    // PC
    public void OnTestOpenPC()
    {
        TestOpenPC();
        routerPC?.OnTestOpenPC();
        routerTablet?.OnTestOpenPC();
    }

    public void TestOpenPC()
    {
        if (additionalMenuPC)
            additionalMenuPC.SetActive(true);
    }

    public void OnTestClosePC()
    {
        TestClosePC();
        routerPC?.OnTestClosePC();
        routerTablet?.OnTestClosePC();
    }

    public void TestClosePC()
    {
        if (additionalMenuPC)
            additionalMenuPC.SetActive(false);
    }

    // Tablet
    public void OnTestOpenTablet()
    {
        TestOpenTablet();
        routerPC?.OnTestOpenTablet();
        routerTablet?.OnTestOpenTablet();
    }

    public void TestOpenTablet()
    {
        if (additionalMenuTablet)
            additionalMenuTablet.SetActive(true);
    }

    public void OnTestCloseTablet()
    {
        TestCloseTablet();
        routerPC?.OnTestCloseTablet();
        routerTablet?.OnTestCloseTablet();
    }

    public void TestCloseTablet()
    {
        if (additionalMenuTablet)
            additionalMenuTablet.SetActive(false);
    }
}
