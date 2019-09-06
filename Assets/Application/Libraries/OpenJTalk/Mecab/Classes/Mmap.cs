using System ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using OJT ;

namespace MecabForOpenJTalk.Classes
{
	public class Mmap<T>
	{
		private byte[]				m_Data ;
		private readonly Type		m_Type ;
		private readonly int		m_Unit ;

		private	int					m_Length ;

		public Mmap()
		{
			m_Unit = 1 ;

			m_Type = typeof( T ) ;
			if( m_Type == typeof( bool ) )
			{
				m_Unit = 1 ;
			}
			else
			if( m_Type == typeof( byte ) )
			{
				m_Unit = 1 ;
			}
			else
			if( m_Type == typeof( char ) || m_Type == typeof( short ) || m_Type == typeof( ushort ) )
			{
				m_Unit = 2 ;
			}
			else
			if( m_Type == typeof( int ) || m_Type == typeof( uint ) || m_Type == typeof( float ) )
			{
				m_Unit = 4 ;
			}
			else
			if( m_Type == typeof( long ) || m_Type == typeof( ulong ) || m_Type == typeof( double ) )
			{
				m_Unit = 8 ;
			}
		}

		public bool Open( string tPath )
		{
			Close() ;

			m_Length = OpenJTalk_StorageAccessor.GetSize( tPath ) ;
			m_Data = OpenJTalk_StorageAccessor.Load( tPath ) ;

			return true ;
		}

		public int Size
		{
			get
			{
				return m_Length / m_Unit ;
			}
		}

		public byte[] Data
		{
			get
			{
				return m_Data ;
			}
		}

		public T this[ int tIndex ]
		{
			get
			{
				int o = m_Unit * tIndex ;
				int i ;
				
				if( m_Type == typeof( bool ) )
				{
					return ( T )( ( object )( m_Data[ o ] != 0 ) ) ;
				}
				else
				if( m_Type == typeof( byte ) )
				{
					return ( T )( ( object )m_Data[ o ] ) ;
				}
				else
				if( m_Type == typeof( short ) )
				{
					short v = 0 ;
					for( i  = 0 ; i <  m_Unit ; i ++ )
					{
						v = ( short )( v | ( short )( m_Data[ o + i ] << ( i * 8 ) ) ) ;
					}
					return ( T )( ( object )v ) ;
				}
				else
				if( m_Type == typeof( char ) || m_Type == typeof( ushort ) )
				{
					ushort v = 0 ;
					for( i  = 0 ; i <  m_Unit ; i ++ )
					{
						v = ( ushort )( v | ( ushort )( m_Data[ o + i ] << ( i * 8 ) ) ) ;
					}
					return ( T )( ( object )v ) ;
				}
				else
				if( m_Type == typeof( int ) )
				{
					int v = 0 ;
					for( i  = 0 ; i <  m_Unit ; i ++ )
					{
						v = ( int )( v | ( int )( m_Data[ o + i ] << ( i * 8 ) ) ) ;
					}
					return ( T )( ( object )v ) ;
				}
				else
				if( m_Type == typeof( uint ) )
				{
					uint v = 0 ;
					for( i  = 0 ; i <  m_Unit ; i ++ )
					{
						v = ( uint )( v | ( uint )( m_Data[ o + i ] << ( i * 8 ) ) ) ;
					}
					return ( T )( ( object )v ) ;
				}
				else
				if( m_Type == typeof( float ) )
				{
					float v = 0 ;
					if( BitConverter.IsLittleEndian == true )
					{
						v = BitConverter.ToSingle( m_Data, o ) ;
					}
					else
					{
						byte[] b = new byte[ m_Unit ] ;
						for( i  = 0 ; i <  m_Unit ; i ++ )
						{
							b[ i ] = m_Data[ o + m_Unit - 1 - i ] ;
						}
						v = BitConverter.ToSingle( b, 0 ) ;
					}
					return ( T )( ( object )v ) ;
				}
				else
				if( m_Type == typeof( long ) )
				{
					long v = 0 ;
					for( i  = 0 ; i <  m_Unit ; i ++ )
					{
						v = ( v | ( ( long )m_Data[ o + i ] << ( i * 8 ) ) ) ;
					}
					return ( T )( ( object )v ) ;
				}
				else
				if( m_Type == typeof( ulong ) )
				{
					ulong v = 0 ;
					for( i  = 0 ; i <  m_Unit ; i ++ )
					{
						v = ( ulong )( v | ( ulong )( ( ulong )m_Data[ o + i ] << ( i * 8 ) ) ) ;
					}
					return ( T )( ( object )v ) ;
				}
				else
				if( m_Type == typeof( double ) )
				{
					double v = 0 ;

					if( BitConverter.IsLittleEndian == true )
					{
						v = BitConverter.ToDouble( m_Data, o ) ;
					}
					else
					{
						byte[] b = new byte[ m_Unit ] ;
						for( i  = 0 ; i <  m_Unit ; i ++ )
						{
							b[ i ] = m_Data[ o + m_Unit - 1 - i ] ;
						}
						v = BitConverter.ToDouble( b, 0 ) ;
					}
					return ( T )( ( object )v ) ;
				}

				return default( T ) ;
			}
		}

		public void Close()
		{
			// ファイルを直接クローズする

			// ファイルの中身を text に入れているようだ
		}
	}
}
