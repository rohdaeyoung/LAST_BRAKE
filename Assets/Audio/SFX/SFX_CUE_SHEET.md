# LAST BRAKE SFX Pack

Generated original WAV assets for LAST_BRAKE. These are procedural, royalty-free project assets created for this workspace.

## Recommended hookups
- UI/SFX_UI_Select.wav: normal choice click
- UI/SFX_UI_DangerConfirm.wav: risky choice confirm
- UI/SFX_UI_ForcedClick.wav: forced bad choice animation
- FX/SFX_RedWarning_Hit.wav: StatManager riskDelta > 0 / RedWarning
- FX/SFX_Glitch_Burst.wav: addictDelta > 0 and ADDICT >= 60 / glitch burst
- FX/SFX_BlurPulse_Whoosh.wav: intDelta < -10 / blur pulse
- Phone/SFX_Phone_Notify.wav: Step3 KakaoTalk message
- Objects/SFX_Pill_Rattle.wav: pill bottle cut-in
- Body/SFX_Heartbeat_Ramp.wav + Body/SFX_Tinnitus_Ring.wav: Step2 and Step5 drug reaction
- Objects/SFX_Door_Knock.wav: NormalEnd Seoa outside the door
- Ending/SFX_Police_Siren.wav: BadEndSequence.sirenSFX
- Ending/SFX_TrueEnd_StareLoop.wav: TrueEnd face stare phase

## Connected in project
- `SFXManager` has been added to every scene and linked to these clips.
- Dialogue line cues are assigned through `DialogueLine.sfxEffect`.
- Choice clicks, forced choices, stat changes, CG/object cut-ins, stat reveals, BadEnd siren, and TrueEnd stare/tap cues are called from code.
- Re-run `LAST BRAKE > 31. SFX 전체 연결` after replacing or renaming audio files.

## BGM
- `Assets/Audio/BGM/BGM_LastBrake_EerieBed_Loop.wav`: default low-volume eerie bed. It is rendered as a no-dead-air seamless loop, auto-starts from the main menu, and keeps playing continuously across scene loads, dialogue advances, and choices through `BGMController`.
- `Assets/Audio/BGM/BGM_Club_Muffled_Loop.wav`: linked as an optional manual variant, but no longer swapped in automatically so the base music does not dip out between scenes.
- `Assets/Audio/BGM/BGM_Distorted_Risk_Loop.wav`: linked as an optional manual variant. Risk distortion now keeps the base track running and uses mixer parameters when available.

## Folder structure
- UI: buttons, choice, stat reveal
- Phone: notifications and 1393 dial tone
- Objects: pill bottle, glass, report paper, door
- Body: heartbeat, tinnitus, panic breath
- FX: warning, glitch, blur, message reveal
- Ending: siren, cuffs, radio, true-end cuts
- Ambient: scene room tones and club/hospital beds
