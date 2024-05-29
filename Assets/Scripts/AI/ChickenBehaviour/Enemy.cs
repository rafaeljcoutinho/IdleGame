using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Random = UnityEngine.Random;

public class Enemy : BaseInteractableNodeMonobehaviour, IDamageSource
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private AnimatorWrapper animatorWrapper;
    [SerializeField] private float speed;
    [SerializeField] private EnemyHpController hpController;
    [SerializeField] private ActionNodeData nodeData;
    [SerializeField] private AnimationEventDispatcher animationEventDispatcher;
    [SerializeField] private float speedRotation;
    [SerializeField] private bool isAgressive;
    [SerializeField] private bool fightBack ;
    [SerializeField] private int damage = 1;
    [SerializeField] private Transform dropAnchor;

    private EnemyManager enemySpawner;
    private FSM<FSMBaseState> behaviourMachine;
    private Dictionary<SkillData.Type, float> allDamageReceivedWithType = new();
    private float maxDeltaTime = 1f / 30f;

    public bool IsAlive => hpController.IsAlive;
    public float Speed => speed;
    public override NodeData NodeData => nodeData;
    public Animator Animator => animatorWrapper.animator;
    public CharacterController Controller => characterController;
    public EnemyManager EnemySpawner => enemySpawner;
    public override Type PlayerBehaviour => typeof(FightBehaviour);
    private int level;
    public EnemyHpController HpController => hpController;

    public IDamageReceiver damageReceiver => HpController;
    public event Action<SkillService.OverkillInfo> OnAfterEnemyDeath;
    public int Level => level;

    private void Start()
    {
        myFrame = Random.Range(0, skipFramesBehaviorUpdate);
        animatorWrapper.Init();
        characterController.SetFrameInterval(4);
    }
    
    public void Flee()
    {
        hpController.ReturnStatusBar();
        animatorWrapper.animator.SetTrigger("Death");
        Destroy(this.gameObject, animatorWrapper.GetAnimationDuration("DeathAnimationChicken"));
    }

    public void Instakill()
    {
        hpController.Instakill();
        var damageNumberView = OverlayCanvas.Instance.DisposableViewPool.Get<DamageNumberView>();
        damageNumberView.Show(new DamageNumberView.ViewData
        {
            color = Color.red,
            text = $"<size=90%><b>INSTAKILL!</b>",
            spread = .1f,
        }, transform, Vector3.up * 1);
        OverlayCanvas.Instance.DisposableViewPool.Return(damageNumberView, .4f);
        
        animatorWrapper.animator.WriteDefaultValues();
        hpController.ReturnStatusBar();
        DropFromDroptable(SkillData.Type.Character, 1, nodeData, dropAnchor, allDamageReceivedWithType);
        animatorWrapper.animator.SetTrigger("Death");
        Services.Container.Resolve<OverkillService>().NotifyEnemyDied(NodeData);
        Destroy(this.gameObject, animatorWrapper.GetAnimationDuration("DeathAnimationChicken"));
    }
    
    private void OnDeath(SkillService.OverkillInfo overkillInfo)
    {
        Services.Container.Resolve<OverkillService>().NotifyEnemyDied(NodeData);
        CalculateXpPerDamageType();
        animatorWrapper.animator.WriteDefaultValues();
        hpController.ReturnStatusBar();

        var dropCount = overkillInfo.OverkillLogarithim; 
        DropFromDroptable(SkillData.Type.Character, dropCount, nodeData, dropAnchor, allDamageReceivedWithType);
        animatorWrapper.animator.SetTrigger("Death");
        OnAfterEnemyDeath?.Invoke(overkillInfo);
        Destroy(this.gameObject, animatorWrapper.GetAnimationDuration("DeathAnimationChicken"));
    }

    private void HandleDamageType(float damage){
        var weapon = Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager
            .GetItemOnSlot(Equipment.EquipSlot.MainHand) as Equipment;

        var damageType = weapon ? weapon.MainType : SkillData.Type.Melee;

        if(allDamageReceivedWithType.ContainsKey(damageType)){
            allDamageReceivedWithType[damageType] += damage;
            return;
        }
        allDamageReceivedWithType.Add(damageType, damage);
    }

    private void CalculateXpPerDamageType(){
        float totalDamageDone = 0;
        foreach(KeyValuePair<SkillData.Type, float> keyValuePair in allDamageReceivedWithType){
            totalDamageDone += keyValuePair.Value;
        }
        var keys = allDamageReceivedWithType.Keys.ToList();
        foreach(SkillData.Type key in keys){
            allDamageReceivedWithType[key] = allDamageReceivedWithType[key] / totalDamageDone;
        }

    }

    public void OnTakeDamage(HitInfo hitInfo)
    {
        HandleDamageType(hitInfo.Damage);
        var anim = animatorWrapper.animator.GetCurrentAnimatorStateInfo(0);
        if(anim.IsName("attackAnimationChicken")){
            animatorWrapper.animator.SetTrigger("TakeDamage");
        }
        
        if (fightBack && hitInfo.damageSource.damageReceiver != null)
        {
            var fightBehaviour = behaviourMachine.GetState<FightBehavior>();
            fightBehaviour.SetTarget(enemySpawner.player);
            behaviourMachine.GoToState(typeof(FightBehavior));
        }
    }

    [SerializeField] private int skipFramesBehaviorUpdate = 3;
    private int myFrame = 0;
    private void Update()
    {
        if (!hpController.IsAlive)
            return;
        animationEventDispatcher.TriggerEvents();
        if (Time.frameCount % skipFramesBehaviorUpdate == myFrame)
        {
            animatorWrapper.animator.SetBool("IsMoving", false);
            behaviourMachine.Update(Time.deltaTime);   
        }
    }

    public void LookAt(Vector3 direction)
    {
        transform.Rotate(transform.up * Time.deltaTime * speedRotation * Vector3.SignedAngle(transform.forward, direction, Vector3.up), Space.World);
    }
    
    public void Move(Vector3 direction)
    {
        animatorWrapper.animator.SetBool("IsMoving", true);
        characterController.Move(direction * Time.deltaTime * speed);
    }

    public void Initialize(Player player, int level, OutlineManager outlineManager)
    {
        this.outlineManager = outlineManager;
        behaviourMachine = new FSM<FSMBaseState>(new List<FSMBaseState> 
        {
            new RoamingState(this, player, transform.position),
            new FightBehavior(this , animationEventDispatcher, damage)
        },typeof(RoamingState));
        behaviourMachine.GetState<RoamingState>().FSM = behaviourMachine;
        hpController.SetMaxHp(nodeData.hp);
        hpController.ResetHp();
        hpController.OnDeath += OnDeath;
        hpController.OnTakeDamage += OnTakeDamage;
        hpController.RegisterDamageReceiver();
        this.level = level;
    }

    public void SetLevel(int level)
    {
        // trigger vfx
        hpController.SetMaxHp(nodeData.hp);
        behaviourMachine.GetState<FightBehavior>().SetDamage(damage);
        var levelUp = this.level < level;
        if (this.level != level)
        {
            var damageNumberView = OverlayCanvas.Instance.DisposableViewPool.Get<DamageNumberView>();
            damageNumberView.Show(new DamageNumberView.ViewData
            {
                color = levelUp ? Color.white : Color.red,
                text = string.Format("<size=75%>Lv.</size>{0}", level),
                spread = .1f,
            }, transform, Vector3.up);
            OverlayCanvas.Instance.DisposableViewPool.Return(damageNumberView, .4f);
        }
        this.level = level;
    }

    public void SetEnemySpawner(EnemyManager enemySpawner)
    {
        this.enemySpawner = enemySpawner;
    }
}