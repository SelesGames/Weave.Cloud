﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18033
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Weave.AccountManagement.Sql.Linq
{
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using System.Data;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;
    using System.Linq.Expressions;
    using System.ComponentModel;
    using System;
    
    
    [global::System.Data.Linq.Mapping.DatabaseAttribute(Name="weave")]
    internal partial class DataClassesDataContext : System.Data.Linq.DataContext
    {
        
        private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
        
    #region Extensibility Method Definitions
    partial void OnCreated();
    partial void InsertUserInfo(UserInfo instance);
    partial void UpdateUserInfo(UserInfo instance);
    partial void DeleteUserInfo(UserInfo instance);
    #endregion
        
        public DataClassesDataContext() : 
                base(global::Weave.AccountManagement.Sql.Properties.Settings.Default.weaveConnectionString, mappingSource)
        {
            OnCreated();
        }
        
        public DataClassesDataContext(string connection) : 
                base(connection, mappingSource)
        {
            OnCreated();
        }
        
        public DataClassesDataContext(System.Data.IDbConnection connection) : 
                base(connection, mappingSource)
        {
            OnCreated();
        }
        
        public DataClassesDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
                base(connection, mappingSource)
        {
            OnCreated();
        }
        
        public DataClassesDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
                base(connection, mappingSource)
        {
            OnCreated();
        }
        
        internal System.Data.Linq.Table<UserInfo> UserInfos
        {
            get
            {
                return this.GetTable<UserInfo>();
            }
        }
    }
    
    [global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.UserInfo")]
    internal partial class UserInfo : INotifyPropertyChanging, INotifyPropertyChanged
    {
        
        private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
        
        private int _Id;
        
        private System.Guid _UserId;
        
        private string _FacebookAuthString;
        
        private string _TwitterAuthString;
        
        private string _MicrosoftAuthString;
        
        private string _GoogleAuthString;
        
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIdChanging(int value);
    partial void OnIdChanged();
    partial void OnUserIdChanging(System.Guid value);
    partial void OnUserIdChanged();
    partial void OnFacebookAuthStringChanging(string value);
    partial void OnFacebookAuthStringChanged();
    partial void OnTwitterAuthStringChanging(string value);
    partial void OnTwitterAuthStringChanged();
    partial void OnMicrosoftAuthStringChanging(string value);
    partial void OnMicrosoftAuthStringChanged();
    partial void OnGoogleAuthStringChanging(string value);
    partial void OnGoogleAuthStringChanged();
    #endregion
        
        public UserInfo()
        {
            OnCreated();
        }
        
        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Id", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
        public int Id
        {
            get
            {
                return this._Id;
            }
            set
            {
                if ((this._Id != value))
                {
                    this.OnIdChanging(value);
                    this.SendPropertyChanging();
                    this._Id = value;
                    this.SendPropertyChanged("Id");
                    this.OnIdChanged();
                }
            }
        }
        
        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_UserId", DbType="UniqueIdentifier NOT NULL")]
        public System.Guid UserId
        {
            get
            {
                return this._UserId;
            }
            set
            {
                if ((this._UserId != value))
                {
                    this.OnUserIdChanging(value);
                    this.SendPropertyChanging();
                    this._UserId = value;
                    this.SendPropertyChanged("UserId");
                    this.OnUserIdChanged();
                }
            }
        }
        
        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_FacebookAuthString", DbType="VarChar(MAX)")]
        public string FacebookAuthString
        {
            get
            {
                return this._FacebookAuthString;
            }
            set
            {
                if ((this._FacebookAuthString != value))
                {
                    this.OnFacebookAuthStringChanging(value);
                    this.SendPropertyChanging();
                    this._FacebookAuthString = value;
                    this.SendPropertyChanged("FacebookAuthString");
                    this.OnFacebookAuthStringChanged();
                }
            }
        }
        
        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_TwitterAuthString", DbType="VarChar(MAX)")]
        public string TwitterAuthString
        {
            get
            {
                return this._TwitterAuthString;
            }
            set
            {
                if ((this._TwitterAuthString != value))
                {
                    this.OnTwitterAuthStringChanging(value);
                    this.SendPropertyChanging();
                    this._TwitterAuthString = value;
                    this.SendPropertyChanged("TwitterAuthString");
                    this.OnTwitterAuthStringChanged();
                }
            }
        }
        
        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_MicrosoftAuthString", DbType="VarChar(MAX)")]
        public string MicrosoftAuthString
        {
            get
            {
                return this._MicrosoftAuthString;
            }
            set
            {
                if ((this._MicrosoftAuthString != value))
                {
                    this.OnMicrosoftAuthStringChanging(value);
                    this.SendPropertyChanging();
                    this._MicrosoftAuthString = value;
                    this.SendPropertyChanged("MicrosoftAuthString");
                    this.OnMicrosoftAuthStringChanged();
                }
            }
        }
        
        [global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_GoogleAuthString", DbType="VarChar(MAX)")]
        public string GoogleAuthString
        {
            get
            {
                return this._GoogleAuthString;
            }
            set
            {
                if ((this._GoogleAuthString != value))
                {
                    this.OnGoogleAuthStringChanging(value);
                    this.SendPropertyChanging();
                    this._GoogleAuthString = value;
                    this.SendPropertyChanged("GoogleAuthString");
                    this.OnGoogleAuthStringChanged();
                }
            }
        }
        
        public event PropertyChangingEventHandler PropertyChanging;
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void SendPropertyChanging()
        {
            if ((this.PropertyChanging != null))
            {
                this.PropertyChanging(this, emptyChangingEventArgs);
            }
        }
        
        protected virtual void SendPropertyChanged(String propertyName)
        {
            if ((this.PropertyChanged != null))
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
#pragma warning restore 1591
