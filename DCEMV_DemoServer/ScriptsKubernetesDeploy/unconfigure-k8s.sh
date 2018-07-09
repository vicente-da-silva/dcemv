# *************************************************************************
# DC EMV
# Open Source EMV
# Copyright (C) 2018  Vicente Da Silva
# 
# This program is free software: you can redistribute it and/or modify
# it under the terms of the GNU Affero General Public License as published
# by the Free Software Foundation, either version 3 of the License, or
# any later version.
# 
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU Affero General Public License for more details.
# 
# You should have received a copy of the GNU Affero General Public License
# along with this program.  If not, see http://www.gnu.org/licenses/
# *************************************************************************

kubectl delete secret payloolacontreg.azurecr.io -n payloola
kubectl delete secret payloola-cert-secret -n payloola

kubectl delete deployment default-backend -n payloola 
kubectl delete service default-backend  -n payloola 

kubectl delete configmap nginx-ingress-controller-configmap  -n payloola 
kubectl delete deployment nginx-ingress-controller-deployment  -n payloola 
kubectl delete service nginx-ingress-service -n payloola 
kubectl delete ingress nginx-ingress-service -n payloola 

kubectl delete deployment dcemvdemoserver-deployment -n payloola 
kubectl delete service dcemvdemoserver-service -n payloola 
kubectl delete ingress dcemvdemoserver-service -n payloola 

# this will delete everything in the namespace if called first
# kubectl delete namespace payloola 
