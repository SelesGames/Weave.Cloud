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

namespace RssAggregator.Data.Sql
{
    using System;
    using System.ComponentModel;
    using System.Data.Linq.Mapping;
	
	
	[global::System.Data.Linq.Mapping.DatabaseAttribute(Name="weave")]
	public partial class DataClassesDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Extensibility Method Definitions
    partial void OnCreated();
    partial void InsertNewsItem(NewsItem instance);
    partial void UpdateNewsItem(NewsItem instance);
    partial void DeleteNewsItem(NewsItem instance);
    #endregion
		
		public DataClassesDataContext() : 
				base(global::RssAggregator.Data.Sql.Properties.Settings.Default.weaveConnectionString, mappingSource)
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
		
		public System.Data.Linq.Table<NewsItem> NewsItems
		{
			get
			{
				return this.GetTable<NewsItem>();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.NewsItem")]
	public partial class NewsItem : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private System.Guid _Id;
		
		private System.Guid _FeedId;
		
		private System.DateTime _PublishDateTime;
		
		private string _Title;
		
		private string _OriginalPublishDateTimeString;
		
		private string _Link;
		
		private string _ImageUrl;
		
		private string _Description;
		
		private string _YoutubeId;
		
		private string _VideoUri;
		
		private string _PodcastUri;
		
		private string _ZuneAppId;
		
		private string _OriginalRssXml;
		
		private System.Data.Linq.Binary _NewsItemBlob;
		
		private string _UtcPublishDateTimeString;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIdChanging(System.Guid value);
    partial void OnIdChanged();
    partial void OnFeedIdChanging(System.Guid value);
    partial void OnFeedIdChanged();
    partial void OnPublishDateTimeChanging(System.DateTime value);
    partial void OnPublishDateTimeChanged();
    partial void OnTitleChanging(string value);
    partial void OnTitleChanged();
    partial void OnOriginalPublishDateTimeStringChanging(string value);
    partial void OnOriginalPublishDateTimeStringChanged();
    partial void OnLinkChanging(string value);
    partial void OnLinkChanged();
    partial void OnImageUrlChanging(string value);
    partial void OnImageUrlChanged();
    partial void OnDescriptionChanging(string value);
    partial void OnDescriptionChanged();
    partial void OnYoutubeIdChanging(string value);
    partial void OnYoutubeIdChanged();
    partial void OnVideoUriChanging(string value);
    partial void OnVideoUriChanged();
    partial void OnPodcastUriChanging(string value);
    partial void OnPodcastUriChanged();
    partial void OnZuneAppIdChanging(string value);
    partial void OnZuneAppIdChanged();
    partial void OnOriginalRssXmlChanging(string value);
    partial void OnOriginalRssXmlChanged();
    partial void OnNewsItemBlobChanging(System.Data.Linq.Binary value);
    partial void OnNewsItemBlobChanged();
    partial void OnUtcPublishDateTimeStringChanging(string value);
    partial void OnUtcPublishDateTimeStringChanged();
    #endregion
		
		public NewsItem()
		{
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Id", DbType="UniqueIdentifier NOT NULL", IsPrimaryKey=true)]
		public System.Guid Id
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
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_FeedId", DbType="UniqueIdentifier NOT NULL")]
		public System.Guid FeedId
		{
			get
			{
				return this._FeedId;
			}
			set
			{
				if ((this._FeedId != value))
				{
					this.OnFeedIdChanging(value);
					this.SendPropertyChanging();
					this._FeedId = value;
					this.SendPropertyChanged("FeedId");
					this.OnFeedIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_PublishDateTime", DbType="DateTime NOT NULL")]
		public System.DateTime PublishDateTime
		{
			get
			{
				return this._PublishDateTime;
			}
			set
			{
				if ((this._PublishDateTime != value))
				{
					this.OnPublishDateTimeChanging(value);
					this.SendPropertyChanging();
					this._PublishDateTime = value;
					this.SendPropertyChanged("PublishDateTime");
					this.OnPublishDateTimeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Title", DbType="NVarChar(MAX) NOT NULL", CanBeNull=false)]
		public string Title
		{
			get
			{
				return this._Title;
			}
			set
			{
				if ((this._Title != value))
				{
					this.OnTitleChanging(value);
					this.SendPropertyChanging();
					this._Title = value;
					this.SendPropertyChanged("Title");
					this.OnTitleChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_OriginalPublishDateTimeString", DbType="NVarChar(MAX) NOT NULL", CanBeNull=false)]
		public string OriginalPublishDateTimeString
		{
			get
			{
				return this._OriginalPublishDateTimeString;
			}
			set
			{
				if ((this._OriginalPublishDateTimeString != value))
				{
					this.OnOriginalPublishDateTimeStringChanging(value);
					this.SendPropertyChanging();
					this._OriginalPublishDateTimeString = value;
					this.SendPropertyChanged("OriginalPublishDateTimeString");
					this.OnOriginalPublishDateTimeStringChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Link", DbType="NVarChar(MAX) NOT NULL", CanBeNull=false)]
		public string Link
		{
			get
			{
				return this._Link;
			}
			set
			{
				if ((this._Link != value))
				{
					this.OnLinkChanging(value);
					this.SendPropertyChanging();
					this._Link = value;
					this.SendPropertyChanged("Link");
					this.OnLinkChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ImageUrl", DbType="NVarChar(MAX)")]
		public string ImageUrl
		{
			get
			{
				return this._ImageUrl;
			}
			set
			{
				if ((this._ImageUrl != value))
				{
					this.OnImageUrlChanging(value);
					this.SendPropertyChanging();
					this._ImageUrl = value;
					this.SendPropertyChanged("ImageUrl");
					this.OnImageUrlChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Description", DbType="NVarChar(MAX)")]
		public string Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				if ((this._Description != value))
				{
					this.OnDescriptionChanging(value);
					this.SendPropertyChanging();
					this._Description = value;
					this.SendPropertyChanged("Description");
					this.OnDescriptionChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_YoutubeId", DbType="NVarChar(MAX)")]
		public string YoutubeId
		{
			get
			{
				return this._YoutubeId;
			}
			set
			{
				if ((this._YoutubeId != value))
				{
					this.OnYoutubeIdChanging(value);
					this.SendPropertyChanging();
					this._YoutubeId = value;
					this.SendPropertyChanged("YoutubeId");
					this.OnYoutubeIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_VideoUri", DbType="NVarChar(MAX)")]
		public string VideoUri
		{
			get
			{
				return this._VideoUri;
			}
			set
			{
				if ((this._VideoUri != value))
				{
					this.OnVideoUriChanging(value);
					this.SendPropertyChanging();
					this._VideoUri = value;
					this.SendPropertyChanged("VideoUri");
					this.OnVideoUriChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_PodcastUri", DbType="NVarChar(MAX)")]
		public string PodcastUri
		{
			get
			{
				return this._PodcastUri;
			}
			set
			{
				if ((this._PodcastUri != value))
				{
					this.OnPodcastUriChanging(value);
					this.SendPropertyChanging();
					this._PodcastUri = value;
					this.SendPropertyChanged("PodcastUri");
					this.OnPodcastUriChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ZuneAppId", DbType="NVarChar(MAX)")]
		public string ZuneAppId
		{
			get
			{
				return this._ZuneAppId;
			}
			set
			{
				if ((this._ZuneAppId != value))
				{
					this.OnZuneAppIdChanging(value);
					this.SendPropertyChanging();
					this._ZuneAppId = value;
					this.SendPropertyChanged("ZuneAppId");
					this.OnZuneAppIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_OriginalRssXml", DbType="NVarChar(MAX)")]
		public string OriginalRssXml
		{
			get
			{
				return this._OriginalRssXml;
			}
			set
			{
				if ((this._OriginalRssXml != value))
				{
					this.OnOriginalRssXmlChanging(value);
					this.SendPropertyChanging();
					this._OriginalRssXml = value;
					this.SendPropertyChanged("OriginalRssXml");
					this.OnOriginalRssXmlChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_NewsItemBlob", DbType="VarBinary(MAX)", UpdateCheck=UpdateCheck.Never)]
		public System.Data.Linq.Binary NewsItemBlob
		{
			get
			{
				return this._NewsItemBlob;
			}
			set
			{
				if ((this._NewsItemBlob != value))
				{
					this.OnNewsItemBlobChanging(value);
					this.SendPropertyChanging();
					this._NewsItemBlob = value;
					this.SendPropertyChanged("NewsItemBlob");
					this.OnNewsItemBlobChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_UtcPublishDateTimeString", DbType="NVarChar(MAX)")]
		public string UtcPublishDateTimeString
		{
			get
			{
				return this._UtcPublishDateTimeString;
			}
			set
			{
				if ((this._UtcPublishDateTimeString != value))
				{
					this.OnUtcPublishDateTimeStringChanging(value);
					this.SendPropertyChanging();
					this._UtcPublishDateTimeString = value;
					this.SendPropertyChanged("UtcPublishDateTimeString");
					this.OnUtcPublishDateTimeStringChanged();
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
