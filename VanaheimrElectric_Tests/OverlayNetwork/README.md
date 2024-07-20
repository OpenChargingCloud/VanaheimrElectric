# Overlay Network Tests

Charging infrastructure test defaults using an OCPP Overlay Network
consisting of a CSMS, an OCPP Gateway, an OCPP Local Controller,
an Energy Meter at the grid connection point and three Charging Stations.

The HTTP Web Socket connections are initiated in "normal order" from
the Charging Stations to the Local Controller, to the Gateway and
finally to the CSMS.

Between the Charging Stations and the Local Controller the "normal"
OCPP transport JSON array is used. Between the Local Controller and
the Gateway, between the Local Controller and the Energy Meter, and
between the Gateway and the CSMS the OCPP Overlay Network transport
is used.

```
[cs1] ──⭨
[cs2] ───→ [lc] ━━━► [gw] ━━━► [csms]
[cs3] ──🡕    🡔── [em]
```
