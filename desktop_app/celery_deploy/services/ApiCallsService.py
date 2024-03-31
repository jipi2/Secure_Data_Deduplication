import httpx
import json
import requests
import os
import math
from Dto.FileFromCacheDto import FileFromCacheDto, UsersEmailsFileNames, PersonalisedInfoDto, FileFromCacheDto_v2

class ApiCall():
    def __init__(self, baseUrl:str):
        self.api_url = baseUrl
    
    async def callBackendPostMethodDto(self, endPointName:str, token: str, param):
        if token != "":
            headers = {"Authorization": f"Bearer {token}"}
        else:
            headers = {} 
        json = param.model_dump()
        async with httpx.AsyncClient(verify=False) as client:
            response = await client.post(self.api_url+endPointName, headers=headers, json=json)
        return response
    
    async def call_backend(self, endPointName:str, token:str, dtoParam):
        if token != "":
            headers = {"Authorization": f"Bearer {token}"}
        else:
            headers = {}
        params = dtoParam.model_dump()  
        async with httpx.AsyncClient(verify=False) as client:
            response = await client.post(self.api_url+endPointName, headers=headers, json=params)
        return response
    
    async def callBackendGetMethod(self,endPointName:str, token:str):
        headers = {"Authorization": f"Bearer {token}"}
        async with httpx.AsyncClient(verify=False) as client:
            response = await client.get(self.api_url+endPointName, headers=headers)
            return response
    
    async def callBackendPostMethodWithSimpleParams(self, endPointName:str, token:str, param:str):
        headers = {"Authorization": f"Bearer {token}"}
        async with httpx.AsyncClient(verify=False) as client:
            try:
                response = await client.post(self.api_url+endPointName, headers=headers, json=param)
                print(response)
            except Exception as e:
                print(str(e))
        return response
    
    def callBackendPostMethodDtoSYNC(self, endPointName:str, token: str, param):
        if token != "":
            headers = {"Authorization": f"Bearer {token}"}
        else:
            headers = {} 
        json = param.model_dump()
        with httpx.Client(verify=False) as client:
            response = client.post(self.api_url+endPointName, headers=headers, json=json)
        return response
    
    def sendChunkFromFile(self, endPointUploadFIle:str, endPointUploadParams:str,token:str, file_path:str, fileParams:FileFromCacheDto_v2):
        headers = {"Authorization": f"Bearer {token}"}
        print(fileParams)
        json = fileParams.model_dump()
        headers.update({"base64Tag":fileParams.base64Tag})
        chunk_size = 100 * 1024 * 1024  # 100 MB chunk size
        cnt = 0
        with open(file_path, 'rb') as file:
            while True:
                cnt = cnt+1
                chunk = file.read(chunk_size)
                if not chunk:
                    break
                files = {'file': ('chunk', chunk)}
                response = requests.post(self.api_url+endPointUploadFIle, headers=headers,files=files,verify=False, timeout=1200)
                if response.status_code != 200:
                    print("Failed to upload chunk:", response.status_code, response.text)
                    raise Exception("Failed to upload chunk")
        with httpx.Client(verify=False) as client:
            response = client.post(self.api_url+endPointUploadParams, headers=headers, json=json, timeout=1200)
            print(response.status_code)
            if response.status_code != 200:
                print("Failed to upload file params:", response.text)
                raise Exception("Failed to upload file params")
            print('GATA PT UN FISIER')
        return response