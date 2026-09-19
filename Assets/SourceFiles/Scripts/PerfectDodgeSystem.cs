using UnityEngine;

[DisallowMultipleComponent]
public sealed class PerfectDodgeSystem : MonoBehaviour
{
    [SerializeField] private float perfectWindow = 0.16f;
    [SerializeField] private float slowMotionScale = 0.22f;
    [SerializeField] private float slowMotionDuration = 0.32f;
    [SerializeField] private float counterWindow = 0.85f;

    private DodgeController dodgeController;
    private float previousDodgeStateTimer;
    private float slowMotionTimer;

    public bool CounterWindowActive { get; private set; }
    public bool WasPerfectDodge { get; private set; }

    public void Initialize(DodgeController dodge)
    {
        if (dodge == null)
        {
            throw new System.ArgumentNullException(nameof(dodge));
        }

        dodgeController = dodge;
    }

    private void Update()
    {
        if (dodgeController == null)
        {
            return;
        }

        if (slowMotionTimer > 0f)
        {
            slowMotionTimer -= Time.unscaledDeltaTime;
            if (slowMotionTimer <= 0f)
            {
                Time.timeScale = 1f;
                Time.fixedDeltaTime = 0.02f;
            }
        }

        if (CounterWindowActive)
        {
            previousDodgeStateTimer -= Time.unscaledDeltaTime;
            if (previousDodgeStateTimer <= 0f)
            {
                CounterWindowActive = false;
                WasPerfectDodge = false;
            }
        }

        if (!dodgeController.IsDodging)
        {
            return;
        }

        if (dodgeController.DodgeElapsedTime > 0f
            && dodgeController.DodgeElapsedTime <= perfectWindow
            && !WasPerfectDodge)
        {
            TriggerPerfectDodge();
        }
    }

    private void TriggerPerfectDodge()
    {
        WasPerfectDodge = true;
        CounterWindowActive = true;
        previousDodgeStateTimer = counterWindow;

        slowMotionTimer = slowMotionDuration;
        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }
}
