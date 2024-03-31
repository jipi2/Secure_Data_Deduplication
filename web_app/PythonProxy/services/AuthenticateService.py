import os
from dotenv import load_dotenv
from services.ApiCallsService import ApiCall
from Dto.LoginUserDto import LoginUserDto
from Dto.Resp import Resp

load_dotenv()

class ProxyClass():
    def __init__(self):
        self.proxyName = "pythonProxy"
        self.proxyMail = "pythonProxy"
        self.proxyPassword = "proxy"
        self.token = "empty"
        self.gateWay = ApiCall(os.environ.get("backendBaseUrl"))
    
    async def getProxyToken(self):
        # test expiration token and if it is expired, get a new one
        
        response = await self.gateWay.callBackendGetMethod("/api/User/testProxyController", self.token)
        if(response.status_code != 200):
            logProxy = LoginUserDto(Email=self.proxyMail, password=self.proxyPassword)
            response = await self.gateWay.callBackendPostMethodDto("/api/User/login", "", logProxy)        
            resp = Resp(Success=response.json()["succes"], Message=response.json()["message"], AccessToken=response.json()["accessToken"])
            self.token = resp.AccessToken
            print(response.text)

    
    async def getUserEmail(self, userToken):
        response = await self.gateWay.callBackendPostMethodWithSimpleParams("/api/User/GetUserEmail",self.token ,userToken)        
        if(response.status_code != 200):
            return None
        return response.text
    
    def getProxyTokenSYNC(self):
        logProxy = LoginUserDto(Email=self.proxyMail, password=self.proxyPassword)
        response = self.gateWay.callBackendPostMethodDtoSYNC("/api/User/login", "", logProxy)        
        resp = Resp(Success=response.json()["succes"], Message=response.json()["message"], AccessToken=response.json()["accessToken"])
        self.token = resp.AccessToken