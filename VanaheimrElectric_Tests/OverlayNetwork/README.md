# Overlay Network Tests

Charging infrastructure test defaults using an OCPP Overlay Network
consisting of three Charging Stations, an OCPP Local Controller, an
Energy Meter at the shared grid connection point, an OCPP Gateway
and two Charging Station Management Systems.

The HTTP Web Socket connections are initiated in "normal order" from
the Charging Stations to the Local Controller, to the Gateway and
finally to the CSMS.

Between the Charging Stations and the Local Controller the "normal"
OCPP transport JSON array is used. Between the Local Controller and
the Gateway, between the Local Controller and the Energy Meter, and
between the Gateway and the CSMS the OCPP Overlay Network transport
is used.

Both CSMSs have an internal WWCP Roaming Network and an internal
E-Mobility Service Provider (iEMP).

```
[cs1] ──⭨                   🡵 [csms1, RN1] ━━━► [iEMP1]
[cs2] ───→ [lc] ━━━► [gw] ━━━► [csms2, RN1] ━━━► [iEMP2]
[cs3] ──🡕    🡴━ [em]
```
