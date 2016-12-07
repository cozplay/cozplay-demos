// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "GameFramework/Actor.h"
#include "Treasure.generated.h"

UCLASS()
class RAZZLECOZMOUE4_API ATreasure : public AActor
{
	GENERATED_BODY()
	
public:	
	// Sets default values for this actor's properties
	ATreasure();

	// Called when the game starts or when spawned
	virtual void BeginPlay() override;
	
	// Called every frame
	virtual void Tick( float DeltaSeconds ) override;

    UFUNCTION()
    void ShowOutline(bool shouldShow);
    
    UFUNCTION()
    bool IsReached(){ return _isReached; }
    
    UFUNCTION()
    bool IsClaimed(){ return _isClaimed; }
    
    // Only call on active, unclaimed hole
    // Returns true if hole was successfully claimed (only one hole can be claimed at a time) 
    UFUNCTION()
    bool AttemptClaim();
    
    UFUNCTION()
    bool IsActive(){ return _isActive; }
    
    UFUNCTION()
    void SetIsActive(bool isActive);
    
    UFUNCTION()
    void OnReached();
    
    static ATreasure *ClaimedTreasure();
    
private:
    UStaticMeshComponent *_unclaimedHole;
    UStaticMeshComponent *_claimedHole;
    UStaticMeshComponent *_unfilledHole;
    UStaticMeshComponent *_gem;
    UStaticMeshComponent *_outline;
    bool _isReached = false;
    bool _isClaimed = false;
    bool _isActive = false;

    float _reachedDuration = 3.0;
    float _reachedElapsed = 0.0;
    
    float _minWait = 6.0;
    float _maxWait = 12.0;
    float _currentWait;
    float _waitElapsed = 0.0;
    
    float _minActiveDuration = 1.0;
    float _maxActiveDuration = 4.0;
    float _currentActiveDuration;
    float _activeElapsed = 0.0;

    // currently claimed treasure (there can be at most 1, NULL if 0)
    static ATreasure *_claimedTreasure;
};
