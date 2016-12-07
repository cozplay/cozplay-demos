// Fill out your copyright notice in the Description page of Project Settings.

#include "TreasureHunt.h"
#include "CozmoUE.h"
#include "ImageUtils.h"

FCozmoPoseStruct FCozmoPoseStruct::ToUE4()
{
    FCozmoPoseStruct poseUE = *this;
    // Y is flipped
    poseUE.position.Y *= -1.0;
    // mm to cm
    poseUE.position /= 10.0;
    
    return poseUE;
}

// Sets default values
ACozmoUE::ACozmoUE()
{
 	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

}

// Called when the game starts or when spawned
void ACozmoUE::BeginPlay()
{
    Super::BeginPlay();
    _cozmoBridge = FindComponentByClass<UPythonComponent>();
    _cozmoBridge->CallPythonComponentMethod(TEXT("start_cozmo"), TEXT(""));
    UE_LOG(LogTemp, Warning, TEXT("CozmoUE: Opened Cozmo connection"));
}

void ACozmoUE::EndPlay(const EEndPlayReason::Type EndPlayReason)
{
    Super::EndPlay(EndPlayReason);
    _cozmoBridge->CallPythonComponentMethod(TEXT("stop_cozmo"), TEXT(""));
    UE_LOG(LogTemp, Warning, TEXT("CozmoUE: Terminated Cozmo connection"));
}

// Called every frame
void ACozmoUE::Tick(float DeltaTime)
{
	Super::Tick( DeltaTime );
}

void ACozmoUE::RunCozmoCoroutine(FString coroutineCall, UObject *doneUObj, FString doneCall)
{
    // Set object and function for callback upon completion of coroutine
    if (doneUObj && !doneCall.IsEmpty()) {
        _cozmoBridge->SetPythonAttrObject(TEXT("_coroutine_done_uobj"), doneUObj);
        _cozmoBridge->SetPythonAttrString(TEXT("_coroutine_done_call"), doneCall);
    }
    // TODO: Allow return values--of various types. Right now we're using String version of method arbitrarily.
    _cozmoBridge->CallPythonComponentMethodString(TEXT("run_cozmo_coroutine"), coroutineCall);
}

bool ACozmoUE::IsCozmoReady()
{
    _isCozmoReady = _isCozmoReady || _cozmoBridge->CallPythonComponentMethodBool(TEXT("is_cozmo_ready"), TEXT(""));
    return _isCozmoReady;
}

FCozmoPoseStruct ACozmoUE::GetCozmoPose()
{
    if (!IsCozmoReady()) {
        FCozmoPoseStruct pose;
        return pose;
    }
    
    TArray<FString> poseStrings;
    _cozmoBridge->CallPythonComponentMethodStringArray(TEXT("get_cozmo_pose"), TEXT(""), poseStrings);
    if (poseStrings.Num() > 0) {
        FCozmoPoseStruct pose(poseStrings);
        return pose;
    } else {
        FCozmoPoseStruct pose;
        return pose;
    }
}

void ACozmoUE::ForceGoToPosition(float x, float y, UObject *doneUObj, FString doneCall)
{
    if (IsCozmoReady()) {
        RunCozmoCoroutine(FString::Printf(TEXT("self.force_go_to_position(%f, %f)"), x, y), doneUObj, doneCall);
    }
}
