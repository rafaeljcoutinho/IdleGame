using System;
using System.Collections.Generic;
using Cooking;
using DefaultNamespace;
using Fishing;
using Mining;
using UnityEngine;
using Woodcutting;
using Slayer;
using UnityEngine.AI;

public class Player : MonoBehaviour, IDamageSource, IDamageReceiver
{
    [SerializeField] private float speed;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private AnimatorWrapper animator;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private InteractableNodeDetector interactableNodeDetector;
    [SerializeField] private AnimationEventDispatcher animationEventDispatcher;
    [SerializeField] private PlayerItem3DVisuals playerItem3DVisuals;
    [SerializeField] private EnemyHpController hpController;
    [SerializeField] private AttackBehaviourParams attackBehaviourParams;
    [SerializeField] private Cameramod cameramod;
    [SerializeField] private Attribute hpAttribute;
    [SerializeField] private Attribute defAttribute;
    [SerializeField] private EquipmentBehaviour defaultFightBehavior;

    private GameplaySceneBootstrapper bootstrapper;
    private IInteractableNode closestNode;
    private IInteractableNode activeNode;
    public FSM<Behaviour> playerBehaviourFSM { get; private set; }
    private bool interact = false;
    private int level;
    private Vector3 inputs;
    private FightBehaviour fightBehaviour;

    public bool Interact => interact;
    public Vector3 Inputs => inputs;
    public IInteractableNode CurrentNode => closestNode;
    public AnimatorWrapper Animator => animator;
    public Collider Collider => hpController.Collider;
    public AnimationEventDispatcher AnimationEventDispatcher => animationEventDispatcher;
    public CharacterController CharacterController => characterController;
    public PlayerItem3DVisuals PlayerItem3DVisuals => playerItem3DVisuals;
    public EnemyHpController HpController => hpController;
    public IDamageReceiver damageReceiver => this;
    public AttackBehaviourParams AttackBehaviourParams => attackBehaviourParams;
    public EquipmentBehaviour DefaultFightBehavior => defaultFightBehavior;
    public Enemy Enemy => null;

    // events
    public Action OnBehaviourChanged;

    private void Init()
    {
        animator.Init();
        defaultFightBehavior.SetPlayer(this);
        level = Services.Container.Resolve<SkillService>().GetPlayerSkillInfo(SkillData.Type.Character).Level;
        RecalculateMaxHP(level);
        hpController.ResetHp();
        hpController.OnDeath += OnDeath;
        hpController.OnRespawn += OnRespawn;
        hpController.OnHeal += OnHeal;
        Services.Container.Resolve<DamageProcessorService>().RegisterDamageReceiver(hpController.Collider, this);

        fightBehaviour = new FightBehaviour(this);
        playerBehaviourFSM = new FSM<Behaviour>(new List<Behaviour>
        {
            new DoNothingBehaviour(),
            new ChopTreeBehaviour(this, playerBehaviourFSM),
            new CloseDistanceBehaviour(this, playerBehaviourFSM),
            new QuestTalkBehaviour(this),
            new FishingBehaviour(this, playerBehaviourFSM),
            new MiningBehaviour(this),
            new CookingBehaviour(this),
            new SlayerBehaviour(this),
            fightBehaviour,
        }, typeof(DoNothingBehaviour));

        playerBehaviourFSM.GetState<CookingBehaviour>().FSM = playerBehaviourFSM;
        playerBehaviourFSM.GetState<MiningBehaviour>().FSM = playerBehaviourFSM;
        playerBehaviourFSM.GetState<ChopTreeBehaviour>().FSM = playerBehaviourFSM;
        playerBehaviourFSM.GetState<CloseDistanceBehaviour>().FSM = playerBehaviourFSM;
        playerBehaviourFSM.GetState<QuestTalkBehaviour>().FSM = playerBehaviourFSM;
        playerBehaviourFSM.GetState<FishingBehaviour>().FSM = playerBehaviourFSM;
        playerBehaviourFSM.GetState<SlayerBehaviour>().FSM = playerBehaviourFSM;
        Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager.OnEquippedItemChanged +=
            OnEquippedItemChanged;
        Services.Container.Resolve<SkillService>().OnSkillLevelsChanged += OnOnlevelUpCharacter;
    }

    private void OnDestroy()
    {
        Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager.OnEquippedItemChanged -= OnEquippedItemChanged;
        Services.Container.Resolve<SkillService>().OnSkillLevelsChanged -= OnOnlevelUpCharacter;
        OverlayCanvas.Instance.Hud.Button.onClick.RemoveAllListeners();
        Services.Container.Resolve<DamageProcessorService>().UnRegisterDamageReceiver(hpController.Collider);
    }

    void Start()
    {
        OverlayCanvas.Instance.Hud.Button.onClick.AddListener(() =>
        {
            interact = true;
        });
    }

    private void OnHeal(int heal)
    {
        var damageNumberView = OverlayCanvas.Instance.DisposableViewPool.Get<DamageNumberView>();
        var ok = ColorUtility.TryParseHtmlString("#33FF33", out var color);
        damageNumberView.Show(new DamageNumberView.ViewData
        {
            color = ok ? color : Color.white,
            text = $"<size=60%>+{heal}",
            spread = .05f,
        }, transform, Vector3.up * 2f);
        OverlayCanvas.Instance.DisposableViewPool.Return(damageNumberView, .4f);
    }

    private void OnRespawn()
    {
        Services.Container.Resolve<DamageProcessorService>().RegisterDamageReceiver(hpController.Collider, this);
        animator.animator.SetTrigger("respawn");
    }

    private void OnDeath(SkillService.OverkillInfo _)
    {
        Services.Container.Resolve<DamageProcessorService>().UnRegisterDamageReceiver(hpController.Collider);
        animator.animator.SetTrigger("death");
    }

    public void Initialize(GameplaySceneBootstrapper gameplaySceneBootstrapper, Vector3 worldPosition)
    {
        cameramod.Init(gameplaySceneBootstrapper.Camera);
        bootstrapper = gameplaySceneBootstrapper;
        Init();
        transform.position = worldPosition;
        if (characterController != null)
        {
            characterController.Rb.RestPosition(worldPosition, true);
            characterController.Rb.StopHorizontal();
            characterController.Rb.StopVerticalMovement();
        }
        GetComponent<MapClickListener>().SetCamera(gameplaySceneBootstrapper.Camera);
    }

    private void OnEquippedItemChanged(PlayerEquipmentManager.EquippedItemsChangedArgs obj)
    {
        if (Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager.IsFoodSlot(obj.Slot))
        {
            return;
        }

        ResetFSM();
    }

    private void Update()
    {
        PlayerItem3DVisuals.StartPlayerBehavior();
        animator.animator.SetBool("moving", false);
        if (!HpController.IsAlive)
        {
            hitsToProcess.Clear();
            return;   
        }

        TryUseHealingFood();
        
        var mapClickListener = GetComponent<MapClickListener>();
        var validClick = mapClickListener.HasClick;
        if (validClick)
        {
            var pointingOver = OverlayCanvas.Instance.IsPointerOverUIObject();
            validClick &= !pointingOver;
        }
        
        if (Input.GetKeyDown(KeyCode.A))
        {
            var npc = GameObject.Find("Oldmancontainer");
            validClick = true;
            mapClickListener.Node = npc.GetComponent<BaseInteractableNodeMonobehaviour>();
        }
        
        var oldNode = closestNode;
        if (validClick)
        {
            closestNode = mapClickListener.Node;
            if (activeNode != closestNode)
                ResetFSM();
        }

        if (closestNode != oldNode)
        {
            oldNode?.DeHighlight();
            closestNode?.Highlight();
        }

        if (closestNode != null && validClick)
        {
            activeNode = closestNode;
            activeNode.Interact();
            var nodeBehaviour = playerBehaviourFSM.GetState(activeNode.PlayerBehaviour);
            playerBehaviourFSM.GoToState(activeNode.PlayerBehaviour);
            nodeBehaviour.SetInteractableNode(activeNode);
            OverlayCanvas.Instance.Hud.UpdateInteractionNode(null);
            OnBehaviourChanged?.Invoke();
        }
        else if (validClick && mapClickListener.Node == null)
        {
            if (NavMesh.SamplePosition(mapClickListener.Position, out var hit, 2, NavMesh.AllAreas))
            {
                var nodeBehaviour = playerBehaviourFSM.GetState<CloseDistanceBehaviour>();
                nodeBehaviour.SetTargetPosition(hit.position, .1f, typeof(DoNothingBehaviour));
                playerBehaviourFSM.GoToState(typeof(CloseDistanceBehaviour));
                OverlayCanvas.Instance.Hud.UpdateInteractionNode(null);
                OnBehaviourChanged?.Invoke();   
            }
        }
        
        AnimationEventDispatcher.TriggerEvents();
        playerBehaviourFSM.Update(Time.deltaTime);

        var grounded = characterController.Rb.Grounded || characterController.previousGrounded;
        var onAir = !grounded;
        animator.animator.SetBool("falling", onAir);
        interact = false;

        var offlineTracker = Services.Container.Resolve<InventoryService>();
        offlineTracker.PlayerProfile.OfflineTracker.RegisterActivity(activeNode, this);
        foreach (var hits in hitsToProcess)
        {
            PlayerItem3DVisuals.TakeDamage(hits);            
        }
        PlayerItem3DVisuals.FinishPlayerBehavior(Time.deltaTime, bootstrapper);
        hitsToProcess.Clear();
    }
    
    void TryUseHealingFood()
    {
        var needsHealing = HpController.NormalizedHp < .6f;
        if (!needsHealing)
            return;
        
        var equipManager = Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager;
        var equippedItems = equipManager.GetAllEquippedItems();
        foreach (var item in equippedItems)
        {
            if (item is not EquipableFood)
                continue;
            var food = item as EquipableFood;
            if (food.HealingAttributes.HpRecovered <= 0)
            {
                continue;
            }

            if (!food.Accept(Services.Container.Resolve<ConsumableItemResolver>().SetContext(this, ConsumableItemResolver.Source.Equipment)))
            {
                continue;
            }
            return;
        }
    }

    public SkillData.Type CurrentSkillBeingUsed()
    {
        return playerBehaviourFSM.GetState(playerBehaviourFSM.CurrentState).SkillUsed;
    }

    public Behaviour GetBehaviour(Type type)
    {
        return playerBehaviourFSM.GetState(type);
    }

    public void ResetFSM()
    {
        playerBehaviourFSM.GoToState(typeof(DoNothingBehaviour));
        PlayerItem3DVisuals.ResetVisualsDefaults();
        activeNode = null;
        OnBehaviourChanged?.Invoke();
    }

    private void RefreshClosestNode()
    {
        var mapClickListener = GetComponent<MapClickListener>();
        if (closestNode == null && mapClickListener.Node != null)
        {
            closestNode = mapClickListener.Node;
        }
        else if (closestNode != null && mapClickListener.HasClick && OverlayCanvas.Instance.Hud.node == null)
        {
            closestNode = null;
        }
        OverlayCanvas.Instance.Hud.UpdateInteractionNode(closestNode);
    }

    private void OnDrawGizmos()
    {
        playerBehaviourFSM.GetState<CloseDistanceBehaviour>().OnDrawGizmos();
    }

    private Vector3 ProcessKeyboardInputs()
    {
        Vector3 ans = Vector3.zero;
        var joystick = Services.Container.Resolve<OverlayCanvas>().Hud.Joystick;
        var cam = bootstrapper.Camera;

        ans += joystick.Direction.x * cam.transform.right;
        ans += joystick.Direction.z * cam.transform.up;
        if (Input.GetKey(KeyCode.DownArrow))
        {
            ans += -cam.transform.up;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            ans += -cam.transform.right;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            ans += cam.transform.right;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            ans += cam.transform.up;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            interact = true;
        }

        return ans;
    }

    public void MoveAndRotateCharacter(Vector3 projectedOnGround)
    {
        animator.animator.SetBool("moving", true);
        Move(projectedOnGround);
        CharacterLookAt(projectedOnGround);
    }

    public void Move(Vector3 diretion)
    {
        characterController.Move(diretion * Time.deltaTime * speed);
    }

    public void CharacterLookAt(Vector3 direction, float speedMulti = 1f)
    {
        var angle = Mathf.Clamp01(Time.deltaTime * speedMulti * rotateSpeed);
        transform.Rotate(
            Vector3.up * angle *
            Vector3.SignedAngle(transform.forward, direction, Vector3.up), Space.World);
    }

    private List<HitInfo> hitsToProcess = new List<HitInfo>();
    public SkillService.OverkillInfo TakeDamage(HitInfo hitInfo)
    {
        var baseDamage = hitInfo.Damage;
        var finalDamage = Mathf.RoundToInt(baseDamage * (1 - GetPercentResistence(level)));
        hitInfo.Damage = finalDamage;
        
        var damageNumberView = OverlayCanvas.Instance.DisposableViewPool.Get<DamageNumberView>();
        var ok = ColorUtility.TryParseHtmlString("#FD6B40", out var color);
        damageNumberView.Show(new DamageNumberView.ViewData
        {
            color = ok ? color : Color.white,
            text = $"<size=110%>{hitInfo.Damage}",
            spread = .05f,
        }, transform, Vector3.up * 1f);
        OverlayCanvas.Instance.DisposableViewPool.Return(damageNumberView, .4f);
        hpController.TakeDamage(hitInfo);

        if (hpController.IsAlive)
        {
            hitsToProcess.Add(hitInfo);
        }
        return null;
    }
    
    public void OnOnlevelUpCharacter(List<SkillService.LevelupEvent> levelupEvents)
    {
        foreach (var levelupEvent in levelupEvents)
        {
            if (levelupEvent.skill != SkillData.Type.Character)
            {
                return;
            }
            level = levelupEvent.newLevel;
            RecalculateMaxHP(level);
            hpController.Heal(hpController.MaxHp - hpController.CurrentHp);
        }
    }

    private void RecalculateMaxHP(int level)
    {
        var levelModifiers = Services.Container.Resolve<SkillService>()
            .GetParametersForLevel(SkillData.Type.Character, level);
        var equippedItems = Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager
            .GetAllEquippedItems();
        var myAttributes = Services.Container.Resolve<SkillService>().GetCombinedModifiers(equippedItems, null);
        myAttributes.Combine(levelModifiers);
        var myHp = myAttributes.GetValueForAttribute(hpAttribute, 1);
        hpController.SetMaxHp(Mathf.CeilToInt(myHp));
    }

    private float GetPercentResistence(int level)
    {
        var levelModifiers = Services.Container.Resolve<SkillService>()
            .GetParametersForLevel(SkillData.Type.Character, level);
        var equippedItems = Services.Container.Resolve<InventoryService>().PlayerProfile.PlayerEquipmentManager
            .GetAllEquippedItems();
        var myAttributes = Services.Container.Resolve<SkillService>().GetCombinedModifiers(equippedItems, null);
        myAttributes.Combine(levelModifiers);
        var def = myAttributes.GetValueForAttribute(defAttribute, 0);
        return def;
    }

    public abstract class Behaviour : IFSMState
    {
        public abstract Type Update(float dt);
        public abstract void SetInteractableNode(IInteractableNode node);

        public virtual void OnForceChange()
        {
        }

        public virtual bool AllowFSMReset => true;

        public FSM<Behaviour> FSM { get; set; }
        public abstract SkillData.Type SkillUsed { get; }
    }
}