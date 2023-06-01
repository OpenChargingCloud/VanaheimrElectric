#!/bin/bash

git submodule foreach git pull
git pull
dotnet build ChargeITMobilityCSO1_Prod.sln
