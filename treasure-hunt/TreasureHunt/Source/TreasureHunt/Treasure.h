#pragma once

#include "GameFramework/Actor.h"
#include "Treasure.generated.h"

UCLASS()
class TREASUREHUNT_API ATreasure : public AActor
{
	GENERATED_BODY()
	
public:	
	// Sets default values for this actor's properties
	ATreasure();

	// Called when the game starts or when spawned
	virtual void BeginPlay() override;
	
	// Called every frame
	virtual void Tick( float DeltaSeconds ) override;

    // True if an outline should be displayed around the treasure spot
    UFUNCTION()
    void ShowOutline(bool shouldShow);
    
    // True if the treasure spot is claimed and has been reached by Cozmo
    UFUNCTION()
    bool IsReached(){ return _isReached; }
    
    // True if the treasure spot has been claimed
    UFUNCTION()
    bool IsClaimed(){ return _isClaimed; }
    
    // This should only be called on an active, unclaimed treasure.
    // Returns true if treasure was successfully claimed (only one treasure can be claimed at a time)
    UFUNCTION()
    bool AttemptClaim();
    
    // True if the treasure spot should be visible
    UFUNCTION()
    bool IsActive(){ return _isActive; }
    
    // Show or hide the treasure spot
    UFUNCTION()
    void SetIsActive(bool isActive);
    
    // Callback for when Cozmo reaches a claimed treasure spot
    UFUNCTION()
    void OnReached();
    
    // Returns currently claimed treasure (static, as only one treasure at most can be claimed at a time)
    static ATreasure *ClaimedTreasure();
    
private:
    UStaticMeshComponent *_unclaimedSpot;
    UStaticMeshComponent *_claimedSpot;
    UStaticMeshComponent *_claimedHole;
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
