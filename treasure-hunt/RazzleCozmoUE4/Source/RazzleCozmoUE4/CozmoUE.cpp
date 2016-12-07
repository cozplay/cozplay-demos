// Fill out your copyright notice in the Description page of Project Settings.

#include "RazzleCozmoUE4.h"
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
    
    // NOTE: Short circuit avoids unneeded Python calls
    if (!_didInitialCubeCheck && IsCozmoReady()) {
        _didInitialCubeCheck = true;
        if (_initialCubeCheck > 0) {
            UObject *callback_uobj = NULL;
            FString callback_call = FString(TEXT(""));
            if (_shouldRunSample) {
                callback_uobj = this;
                callback_call = FString(TEXT("RunCozmoCoroutine FString(\"self.sample_coroutine1()\") this FString(\"SampleCallback1\")"));
            }
            RunCozmoCoroutine(FString::Printf(TEXT("self.wait_for_cubes_observed(count=%d, timeout=%d)"),
                                              _initialCubeCheck,
                                              _initialCubeCheckTimeout),
                                              callback_uobj,
                                              callback_call);
        } else if (_shouldRunSample) {
            RunCozmoCoroutine(TEXT("self.sample_coroutine1()"), this, TEXT("SampleCallback1"));
        }
    }
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

bool ACozmoUE::IsCubeVisible(int index)
{
    if (!IsCozmoReady()) {
        return false;
    }
    return false;
}

FCozmoPoseStruct ACozmoUE::GetCubePose(int index)
{
    if (!IsCozmoReady()) {
        FCozmoPoseStruct pose;
        return pose;
    }
    
    FCozmoPoseStruct pose;
    return pose;
}

void ACozmoUE::UpdateCameraFeed()
{
    FString pixels = _cozmoBridge->CallPythonComponentMethodString(TEXT("get_camera_feed"), TEXT(""));
//    size_t feedSize = 320*240*4;
//    uint8 buffer[feedSize];
//    FString::ToBlob(pixels, buffer, feedSize);
    
    TArray<TCHAR> bytes = pixels.GetCharArray();
    UE_LOG(LogTemp, Warning, TEXT("PIXELS LENGTH: %d"), pixels.Len());
    UE_LOG(LogTemp, Warning, TEXT("BYTES LENGTH: %d"), bytes.Num());
    //FImageUtils::CreateTexture2D(320, 240, <#const TArray<FColor> &SrcData#>, <#UObject *Outer#>, <#const FString &Name#>, <#const EObjectFlags &Flags#>, <#const FCreateTexture2DParameters &InParams#>)
    //RHICreateTexture2D(320, 240, uint8 Format, uint32 NumMips, uint32 NumSamples, uint32 Flags, FRHIResourceCreateInfo &CreateInfo)
}

void ACozmoUE::SampleCallback1()
{
    UE_LOG(LogTemp, Warning, TEXT("CozmoUE: Sample Callback 1/2"));
    UE_LOG(LogTemp, Warning, TEXT("Three"));
    UE_LOG(LogTemp, Warning, TEXT("Four"));
    RunCozmoCoroutine(TEXT("self.sample_coroutine2()"), this, TEXT("SampleCallback2"));
}

void ACozmoUE::SampleCallback2()
{
    // UpdateCameraFeed();
    UE_LOG(LogTemp, Warning, TEXT("CozmoUE: Sample Callback 2/2"));
    UE_LOG(LogTemp, Warning, TEXT("Seven"));
    UE_LOG(LogTemp, Warning, TEXT("Eight"));
}

void ACozmoUE::ForceGoToPosition(float x, float y, UObject *doneUObj, FString doneCall)
{
    if (IsCozmoReady()) {
        RunCozmoCoroutine(FString::Printf(TEXT("self.force_go_to_position(%f, %f)"), x, y), doneUObj, doneCall);
    }
}
