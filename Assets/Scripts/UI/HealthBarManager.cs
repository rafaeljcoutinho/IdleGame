using System;
using UnityEngine;

[Serializable]
public class HealthBarManager
{
    [SerializeField] private SingleStatusBar statusBarPrefab;
    [SerializeField] private Transform statusBarContainer;
    [SerializeField] private SkillProgressBar skillProgressBarPrefab;
    [SerializeField] private Transform skillBarContainer;
    
    [SerializeField] private GatheringNodeSkillInfoView skillInfoView;

    private ObjectPool<SingleStatusBar> statusBarPool;
    private ObjectPool<SkillProgressBar> skillProgressPool;

    public void Init()
    {
        ResetBars();
        statusBarPool = new ObjectPool<SingleStatusBar>(statusBarContainer, statusBarPrefab);
        skillProgressPool = new ObjectPool<SkillProgressBar>(skillBarContainer, skillProgressBarPrefab);
    }

    private void ResetBars()
    {
        if (statusBarPool != null)
        {
            foreach (var objects in statusBarPool.UsedObjects)
            {
                objects.Disapear();
            }
        }
        if (statusBarPool != null)
        {
            foreach (var objects in skillProgressPool.UsedObjects)
            {
                objects.Disapear();
            }
        }
    }
    public SkillProgressBar GetPooledSkillProgressBar()
    {
        return skillProgressPool.Pop();
    }
    
    public SingleStatusBar GetPooledStatusBar()
    {
        return statusBarPool.Pop();
    }

    public void OnSkillProgressBarClick(SkillData.Type type)
    {
        var viewModel = skillInfoView.SkillInfoView.ViewData;
        SkillInfoView.ViewModelBuilder.UpdateViewModel(viewModel, type);

        skillInfoView.SkillInfoView.RefreshViewData();
        skillInfoView.gameObject.SetActive(true);
    }

    public void Dispose(SkillProgressBar progressBar)
    {
        progressBar.Dispose(() =>
        {
            skillProgressPool.Push(progressBar);
        });
    }
    
    public void Dispose(SingleStatusBar statusBar)
    {
        statusBar.Dispose(() =>
        {
            statusBarPool.Push(statusBar);
        });
    }
    
}