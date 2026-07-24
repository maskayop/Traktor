using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

/// <summary>
/// Приём head tracking из OpenTrack по UDP (протокол OpenTrack: 6× double).
///
/// OpenTrack:
///   Output = "UDP over network"
///   IP = 127.0.0.1
///   Port = тот же, что в поле port ниже (по умолчанию 5555)
///
/// Пакет: 48 байт = X, Y, Z, Yaw, Pitch, Roll (little-endian double).
/// Углы — градусы. Позицию по умолчанию не применяем (только поворот камеры).
///
/// Использование:
///   1. Повесить на дочерний объект камеры кабины (не на корень вида).
///   2. При смене вида (назад/зеркала) — отключать компонент (enabled = false).
/// </summary>
public class OpenTrackUdp : MonoBehaviour
{
    [Header("OpenTrack UDP")]
    public int port = 5555;

    [Header("Чувствительность")]
    public float yawScale = 1f;
    public float pitchScale = 1f;
    public float rollScale = 0f; // обычно 0 для кабины

    [Header("Сглаживание")]
    [Tooltip("0 = выкл, 5–12 = мягко, 15+ = очень плавно (с задержкой)")]
    public float smoothSpeed = 8f;

    [Header("Лимиты (градусы), 0 = без лимита")]
    public float maxYaw = 45f;
    public float maxPitch = 30f;

    [Header("Опции")]
    public bool applyPosition = false;
    public float positionScale = 0.01f; // мм OpenTrack → метры Unity (подстроить)

    UdpClient _udp;
    Thread _thread;
    volatile bool _running;

    double _x, _y, _z, _yaw, _pitch, _roll;
    readonly object _lock = new object();

    Quaternion _baseLocalRotation;
    Vector3 _baseLocalPosition;
    Quaternion _smoothedOffset = Quaternion.identity;
    Vector3 _smoothedPosOffset;

    void Awake()
    {
        _baseLocalRotation = transform.localRotation;
        _baseLocalPosition = transform.localPosition;
        _smoothedOffset = Quaternion.identity;
        _smoothedPosOffset = Vector3.zero;
    }

    void OnEnable()
    {
        _smoothedOffset = Quaternion.identity;
        _smoothedPosOffset = Vector3.zero;
        StartReceiver();
    }

    void OnDisable()
    {
        StopReceiver();
        transform.localRotation = _baseLocalRotation;
        transform.localPosition = _baseLocalPosition;
        _smoothedOffset = Quaternion.identity;
        _smoothedPosOffset = Vector3.zero;
    }

    void OnDestroy()
    {
        StopReceiver();
    }

    void StartReceiver()
    {
        StopReceiver();

        try
        {
            _udp = new UdpClient(port);
        }
        catch (Exception e)
        {
            Debug.LogError($"[OpenTrackUdp] Не удалось открыть порт {port}: {e.Message}");
            return;
        }

        _running = true;
        _thread = new Thread(ReceiveLoop)
        {
            IsBackground = true,
            Name = "OpenTrackUdp"
        };
        _thread.Start();
    }

    void StopReceiver()
    {
        _running = false;

        try { _udp?.Close(); } catch { /* ignore */ }
        _udp = null;

        if (_thread != null)
        {
            _thread.Join(500);
            _thread = null;
        }
    }

    void ReceiveLoop()
    {
        var ep = new IPEndPoint(IPAddress.Any, 0);

        while (_running)
        {
            try
            {
                if (_udp == null) break;

                byte[] data = _udp.Receive(ref ep);
                if (data == null || data.Length < 48) continue;

                lock (_lock)
                {
                    _x = BitConverter.ToDouble(data, 0);
                    _y = BitConverter.ToDouble(data, 8);
                    _z = BitConverter.ToDouble(data, 16);
                    _yaw = BitConverter.ToDouble(data, 24);
                    _pitch = BitConverter.ToDouble(data, 32);
                    _roll = BitConverter.ToDouble(data, 40);
                }
            }
            catch (SocketException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception e)
            {
                if (_running)
                    Debug.LogWarning($"[OpenTrackUdp] {e.Message}");
            }
        }
    }

    void Update()
    {
        double x, y, z, yaw, pitch, roll;
        lock (_lock)
        {
            x = _x; y = _y; z = _z;
            yaw = _yaw; pitch = _pitch; roll = _roll;
        }

        float yawDeg = (float)(yaw * yawScale);
        float pitchDeg = (float)(pitch * pitchScale);
        float rollDeg = (float)(roll * rollScale);

        if (maxYaw > 0f) yawDeg = Mathf.Clamp(yawDeg, -maxYaw, maxYaw);
        if (maxPitch > 0f) pitchDeg = Mathf.Clamp(pitchDeg, -maxPitch, maxPitch);

        // OpenTrack: yaw/pitch/roll → Unity localEuler (подстроить знаки при необходимости)
        Quaternion targetOffset = Quaternion.Euler(-pitchDeg, yawDeg, -rollDeg);

        if (smoothSpeed <= 0f)
            _smoothedOffset = targetOffset;
        else
            _smoothedOffset = Quaternion.Slerp(_smoothedOffset, targetOffset, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));

        transform.localRotation = _baseLocalRotation * _smoothedOffset;

        if (applyPosition)
        {
            Vector3 targetPos = new Vector3(
                (float)(x * positionScale),
                (float)(y * positionScale),
                (float)(z * positionScale)
            );

            if (smoothSpeed <= 0f)
                _smoothedPosOffset = targetPos;
            else
                _smoothedPosOffset = Vector3.Lerp(_smoothedPosOffset, targetPos, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));

            transform.localPosition = _baseLocalPosition + _smoothedPosOffset;
        }
    }
}
