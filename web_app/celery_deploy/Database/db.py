from sqlalchemy import create_engine, Column, Integer, String, Sequence, LargeBinary, Table, DateTime, and_
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import sessionmaker

from typing import List
from typing import Optional
from sqlalchemy import ForeignKey
from sqlalchemy import String
from sqlalchemy.orm import DeclarativeBase
from sqlalchemy.orm import Mapped
from sqlalchemy.orm import mapped_column
from sqlalchemy.orm import relationship
import datetime
import os
from dotenv import load_dotenv

load_dotenv()

# engine = create_engine("mysql+mysqlconnector://root:root@localhost:2703/cache")
engine = create_engine(os.environ.get('DB_URL'))

Session = sessionmaker(bind=engine)

class Base(DeclarativeBase):
    def __init__(self):
        self.engine = engine
    
    def getEngine(self):
        return self.engine

    def getSession(self):
        self.session = Session()
        return self.session

    def closeSession(self):
        self.session.close()

class UserFile (Base):
    __tablename__ = 'user_file'
    # Base.metadata,
    id = Column(Integer, Sequence('user_file_id_seq'), primary_key=True)
    user_id = Column('user_id', Integer, ForeignKey('users.id'))
    file_id = Column('file_id', Integer, ForeignKey('files.id'))
    key = Column('key', String(512))
    iv = Column('iv', String(512))
    fileName = Column('fileName', String(2048))
    upload_date = Column('upload_date', DateTime, default=datetime.datetime.utcnow)
    
    user = relationship("User", back_populates="user_files")  # Define relationship with User
    file = relationship("File", back_populates="file_users")  #
    
    def __init__(self, user_id, file_id, key, iv, fileName, date):
        self.user_id = user_id
        self.file_id = file_id
        self.key = key
        self.iv = iv
        self.fileName = fileName
        self.upload_date = date


class User(Base):
    __tablename__ = 'users'
    id = Column(Integer, Sequence('user_id_seq'), primary_key=True)
    email = Column(String(50))
    # files = relationship("File", secondary=UserFile, back_populates="users")
    user_files = relationship("UserFile", back_populates="user") 
    
    def __init__(self, email):
        self.email = email
    
    def __repr__(self):
        return "<User(email='%s')>" % (self.email)

class File(Base):
    __tablename__ = 'files'
    id = Column(Integer, Sequence('file_id_seq'), primary_key=True)
    
    # key = Column(String(100))
    # iv = Column(String(100))
    # fileName = Column(String(200))
    tag = Column(String(100))
    
    blob_file_id = Column(Integer, ForeignKey('blob_files.id'))
    blob_file = relationship('BlobFile', back_populates='file')
    
    file_users = relationship("UserFile", back_populates="file") 
    
    # users = relationship("User", secondary=user, back_populates="files")

    def __init__(self, tag, blob_file=None):
        self.tag = tag
        # self.key = key
        # self.iv = iv
        # self.fileName = fileName
        self.blob_file = blob_file 

class BlobFile(Base):
    __tablename__ = 'blob_files'
    id = Column(Integer, Sequence('blob_id_seq'), primary_key=True)
    base64EncFile = Column(LargeBinary(length=(2**32)-1))
    file = relationship("File", back_populates="blob_file")
    
    def __init__ (self, base64EncFile):
        self.base64EncFile = base64EncFile

Base.metadata.create_all(engine)