#include "TreasureHunt.h"
#include "Treasure.h"

ATreasure * ATreasure::_claimedTreasure = NULL;

// Sets default values
ATreasure::ATreasure()
{
 	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;
}

// Called when the game starts or when spawned
void ATreasure::BeginPlay()
{
	Super::BeginPlay();
    _claimedTreasure = NULL; // Only necessary for editor (to avoid recompiling for static variable reset)
    
    _unclaimedSpot = (UStaticMeshComponent *)GetComponentsByTag(UStaticMeshComponent::StaticClass(), TEXT("UnclaimedSpot"))[0];
    _claimedSpot = (UStaticMeshComponent *)GetComponentsByTag(UStaticMeshComponent::StaticClass(), TEXT("ClaimedSpot"))[0];
    _claimedHole = (UStaticMeshComponent *)GetComponentsByTag(UStaticMeshComponent::StaticClass(), TEXT("ClaimedHole"))[0];
    _gem = (UStaticMeshComponent *)GetComponentsByTag(UStaticMeshComponent::StaticClass(), TEXT("Gem"))[0];
    _outline = (UStaticMeshComponent *)GetComponentsByTag(UStaticMeshComponent::StaticClass(), TEXT("Outline"))[0];
    SetIsActive(false);
    _currentWait = FMath::RandRange(_minWait, _maxWait);
    _currentActiveDuration = FMath::RandRange(_minActiveDuration, _maxActiveDuration);
	ShowOutline(false);
}

// Called every frame
void ATreasure::Tick( float DeltaTime )
{
	Super::Tick( DeltaTime );
    
    // When treasure is claimed hide non-claimed treasure and halt ticking until sequence completes
    if (ClaimedTreasure() != this && ClaimedTreasure() != NULL) {
        if (_isActive) {
            SetIsActive(false);
        }
        return;
    }
    
    if (_isReached) {
        _reachedElapsed += DeltaTime;
        if (_reachedElapsed > _reachedDuration) {
            _reachedElapsed = 0.0;
            SetIsActive(false);
        }
    } else if (!_isActive) {
        _waitElapsed += DeltaTime;
        if (_waitElapsed > _currentWait) {
            _waitElapsed = 0.0;
            SetIsActive(true);
            _currentWait = FMath::RandRange(_minWait, _maxWait);
        }
    } else if (!_isClaimed) { // active but not claimed
        _activeElapsed += DeltaTime;
        if (_activeElapsed > _currentActiveDuration) {
            _activeElapsed = 0.0;
            SetIsActive(false);
            _currentActiveDuration = FMath::RandRange(_minActiveDuration, _maxActiveDuration);
        }
    }
}

void ATreasure::ShowOutline(bool shouldShow)
{
    _outline->SetVisibility(shouldShow);
}

bool ATreasure::AttemptClaim()
{
    if (_claimedTreasure == NULL) {
        _claimedTreasure = this;
        _isClaimed = true;
        _claimedSpot->SetVisibility(true);
        _unclaimedSpot->SetVisibility(false);
        return true;
        
    } else if (_claimedTreasure->IsReached()) {
        _claimedTreasure->SetIsActive(false);
        _claimedTreasure = this;
        _isClaimed = true;
        _claimedSpot->SetVisibility(true);
        _unclaimedSpot->SetVisibility(false);
        return true;
    } else {
        return false;
    }
}

void ATreasure::SetIsActive(bool isActive)
{
    _isActive = isActive;
    if (isActive) {
        _unclaimedSpot->SetVisibility(true);
        _waitElapsed = 0.0;
    } else {
        if (_claimedTreasure == this) {
            _claimedTreasure = NULL;
        }
        _isClaimed = false;
        _claimedSpot->SetVisibility(false);
        _unclaimedSpot->SetVisibility(false);
        _claimedHole->SetVisibility(false);
        _gem->SetVisibility(false);
        _outline->SetVisibility(false);
        _activeElapsed = 0.0;
        _isReached = false;
    }
}

void ATreasure::OnReached()
{
    _claimedSpot->SetVisibility(false);
    _claimedHole->SetVisibility(true);
    _gem->SetVisibility(true);
    _isReached = true;
}

ATreasure * ATreasure::ClaimedTreasure()
{
    return _claimedTreasure;
}
