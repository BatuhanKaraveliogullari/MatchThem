﻿using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdManager : Singleton<AdManager>
{
    public RewardedAd rewardedAd;

    private void Start()
    {
        RequestRewardedAd();
    }

    public void RequestRewardedAd()
    {
        string adUnitIdRewardedAd;

        adUnitIdRewardedAd = "ca-app-pub-6936916138450871/9501682438";

        this.rewardedAd = new RewardedAd(adUnitIdRewardedAd);

        this.rewardedAd.OnAdClosed += HandleRewardedAdClosed;

        AdRequest request = new AdRequest.Builder().Build();

        this.rewardedAd.LoadAd(request);
    }

    public void HandleRewardedAdClosed(object sender, EventArgs args)
    {
        MenuManager.Insatance.StartGame();
    }

    public void WatchAd()
    {
        if (this.rewardedAd.IsLoaded())
        {
            this.rewardedAd.Show();
        }

        RequestRewardedAd();
    }
}
