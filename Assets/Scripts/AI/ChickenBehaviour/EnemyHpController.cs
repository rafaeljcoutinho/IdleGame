using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class EnemyHpController : IDamageReceiver
{
    [SerializeField] private int maxHp;
    [SerializeField] private Collider collider;
    [SerializeField] private Transform anchorPointForHealthbar;
    [SerializeField] private SingleStatusBar.DisplayOptions displayOptions;
    [SerializeField] private bool invertHealthBar;
    [SerializeField] private bool showText;
    [SerializeField] private Enemy enemy;

    private SingleStatusBar singleStatusBar;
    private int currentHp;
    private Coroutine returnStatusBarToPoolRoutine;
    public Action<HitInfo> OnTakeDamage;
    public Action<SkillService.OverkillInfo> OnDeath;
    public Action OnInstaKill;
    public Action OnRespawn;
    public Action<int> OnHeal;
    public Transform transform => collider.transform;
    public EnemyHpController HpController => this;

    public float NormalizedHp => invertHealthBar ? (float)(maxHp-currentHp) / maxHp : (float)currentHp / maxHp;
    public Collider Collider => collider;
    public bool IsAlive => currentHp > 0;
    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;
    public Enemy Enemy => enemy;

    public void Heal(int healAmount)
    {
        if (singleStatusBar == null)
        {
            singleStatusBar = OverlayCanvas.Instance.HealthBarManager.GetPooledStatusBar();
            singleStatusBar.Reset(NormalizedHp, showText ? currentHp + " / " + maxHp : "", anchorPointForHealthbar, displayOptions);
        }
        
        currentHp += healAmount;
        currentHp = Math.Clamp(currentHp, 0, maxHp);
        singleStatusBar.UpdateValueNormalized(NormalizedHp, showText ? currentHp + " / " + maxHp : "");
        if (returnStatusBarToPoolRoutine != null)
        {
            Services.Container.Resolve<CoroutineDispatcher>().StopCoroutine(returnStatusBarToPoolRoutine);
        }
        returnStatusBarToPoolRoutine = Services.Container.Resolve<CoroutineDispatcher>().AfterDelay(3f, ReturnStatusBar);
        OnHeal?.Invoke(healAmount);
    }
    
    public void Respawn(float percent = 1f)
    {
        Heal( Mathf.CeilToInt(maxHp * percent));
        OnRespawn?.Invoke();
    }

    public void Instakill()
    {
        if (singleStatusBar == null)
        {
            singleStatusBar = OverlayCanvas.Instance.HealthBarManager.GetPooledStatusBar();
            singleStatusBar.Reset(NormalizedHp, showText ? currentHp + " / " + maxHp : "", anchorPointForHealthbar, displayOptions);
        }
        
        currentHp = 0;
        singleStatusBar.UpdateValueNormalized(NormalizedHp, showText ? currentHp + " / " + maxHp : "");
        if (returnStatusBarToPoolRoutine != null)
        {
            Services.Container.Resolve<CoroutineDispatcher>().StopCoroutine(returnStatusBarToPoolRoutine);
        }
        returnStatusBarToPoolRoutine = Services.Container.Resolve<CoroutineDispatcher>().AfterDelay(3f, ReturnStatusBar);
        OnInstaKill?.Invoke();
        Services.Container.Resolve<DamageProcessorService>().UnRegisterDamageReceiver(collider);
    }
    
    public SkillService.OverkillInfo TakeDamage(HitInfo hitInfo)
    {
        if(!IsAlive){
            return null;
        }

        if (singleStatusBar == null)
        {
            singleStatusBar = OverlayCanvas.Instance.HealthBarManager.GetPooledStatusBar();
            singleStatusBar.Reset(NormalizedHp, showText ? currentHp + " / " + maxHp : "", anchorPointForHealthbar, displayOptions);
        }

        var wasDead = currentHp <= 0;

        currentHp -= hitInfo.Damage;
        currentHp = Math.Clamp(currentHp, int.MinValue, maxHp);
        singleStatusBar.UpdateValueNormalized(NormalizedHp, showText ? currentHp + " / " + maxHp : "");
        if (returnStatusBarToPoolRoutine != null)
        {
            Services.Container.Resolve<CoroutineDispatcher>().StopCoroutine(returnStatusBarToPoolRoutine);
        }
        returnStatusBarToPoolRoutine = Services.Container.Resolve<CoroutineDispatcher>().AfterDelay(3f, ReturnStatusBar);

        OnTakeDamage?.Invoke(hitInfo);

        if(wasDead && currentHp <= 0){
            Debug.LogError("died again");
        }

        if (currentHp <= 0 && !wasDead)
        {
            var overkillInfo = Services.Container.Resolve<SkillService>().GetOverkillInfo(currentHp, maxHp);
            OnDeath?.Invoke(overkillInfo);
            Services.Container.Resolve<DamageProcessorService>().UnRegisterDamageReceiver(collider);
            return overkillInfo;
        }

        return null;
    }


    
    public void ReturnStatusBar()
    {
        var coroutineRunner = Services.Container.Resolve<CoroutineDispatcher>();
        if(returnStatusBarToPoolRoutine != null)
            coroutineRunner.StopCoroutine(returnStatusBarToPoolRoutine);

        if (singleStatusBar != null)
            OverlayCanvas.Instance.HealthBarManager.Dispose(singleStatusBar);

        singleStatusBar = null;
        returnStatusBarToPoolRoutine = null;
    }

    public void RegisterDamageReceiver()
    {
        Services.Container.Resolve<DamageProcessorService>().RegisterDamageReceiver(collider, this);
    }

    public void SetMaxHp(int maxHp)
    {
        this.maxHp = maxHp;
        if (currentHp > maxHp)
        {
            currentHp = maxHp;
        }
    }
    
    public void ResetHp()
    {
        if (singleStatusBar != null)
        {
            ReturnStatusBar();
        }
        currentHp = maxHp;
    }
}