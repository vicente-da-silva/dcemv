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

#######################################################################################
# When running the cloud shell the first time you will be prompted to create a 
# cloud storage account, do so
#######################################################################################

#######################################################################################
# The domain www.payloola.com as an example, replace with your, see the manual as well
#######################################################################################

#######################################################################################
# https://docs.microsoft.com/en-us/azure/aks/
# Create the cluster in a new resource group
#######################################################################################
az provider register -n Microsoft.Network
az provider register -n Microsoft.Storage
az provider register -n Microsoft.Compute
az provider register -n Microsoft.ContainerService
az group create --name payloola_app --location "west europe"
# create cluster
az aks create --resource-group payloola_app --name payloolaKCluster --node-count 1 --generate-ssh-keys 
# --node-vm-size B1ms
# change later via portal ui, B1ms costs less, good enough for testing

# configure kubectl with the credentials
az aks get-credentials --resource-group payloola_app --name payloolaKCluster
# test the cluster
kubectl get nodes

#######################################################################################
# Create the container registry
#######################################################################################
az acr create --resource-group payloola_app --name payloolacontreg --sku Basic
#######################################################################################
# Get the container registry credentials
#######################################################################################
az acr update -n payloolacontreg --admin-enabled true
# in the azure cloud shell use this command to get the acr login server name
az acr show --name payloolacontreg --query loginServer
#=>payloolacontreg.azurecr.io
# get the password, to use in the set up access to registry for kubernetes command
az acr credential show --name payloolacontreg --query passwords[0].value
#=>your_password

#######################################################################################
# Configuring the cluster
#######################################################################################
# namespace creation
kubectl create namespace payloola
cd clouddrive

# set up access to registry for kubernetes
kubectl create secret docker-registry payloolacontreg.azurecr.io --docker-server="payloolacontreg.azurecr.io" --docker-username="payloolacontreg" --docker-password="your_password" --docker-email=vicentedasilva@outlook.com -n payloola

# cert loading
# make sure this cert is the same that was injected into dcemvdemoserver via DockerFile
kubectl create secret tls payloola-cert-secret --cert=./shared/payloola5.crt --key=./shared/payloola5.key -n payloola
# default services
kubectl create -f ./shared/default-backend-deployment.yaml 
kubectl create -f ./shared/default-backend-service.yaml 

# nginx service creation
kubectl create -f ./shared/configmap_nginx-ingress-controller-config-map.yaml
kubectl create -f ./shared/deployment_nginx-ingress-controller-deployment.yaml 
kubectl create -f ./az/service_nginx-ingress-service.yaml
kubectl create -f ./shared/ingress_nginx-ingress-service.yaml

# dcemvdemoserver app service creation
kubectl create -f ./az/deployment_dcemvdemoserver-deployment.yaml
kubectl create -f ./shared/service_dcemvdemoserver-service.yaml
kubectl create -f ./shared/ingress_dcemvdemoserver-service.yaml 

kubectl get services -n payloola
# update azure www.payloola.com dns with the public ip address

# check pods have started and are running
kubectl get pods -n payloola

# check urls
# https://www.payloola.com/nginx_status
# https://www.payloola.com/swagger

#######################################################################################
# Start the kubernetes dashboard
# must run this in local Azure CLI, not in cloud shell 
#######################################################################################
az aks install-cli
az login
az aks get-credentials --resource-group payloola_app --name payloolaKCluster
az acs kubernetes browse --resource-group payloola_app --name payloolaKCluster

#######################################################################################
# Miscellaneous commands
#######################################################################################
kubectl get ingresses -n payloola
kubectl get deployments -n payloola
kubectl describe ingress nginx-ingress-service -n payloola
kubectl describe deployment dcemvdemoserver-deployment -n payloola
# get a description of pod and logs of pod, replace pod name with your pod name
kubectl describe pods dcemvdemoserver-deployment-7865bd7997-gjgh2 -n payloola
kubectl logs dcemvdemoserver-deployment-9967c5c4b-drb8w -n payloola
# get a terminal in the pod, replace pod name with your pod name
kubectl exec nginx-ingress-controller-deployment-777cb594f8-gdxlp -n kube-system -it bash -n payloola
