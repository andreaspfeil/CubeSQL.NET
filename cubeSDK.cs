//
//         ___              __                        ____  ____     _ __
//        /   |  ____  ____/ /_______  ____ ______   / __ \/ __/__  (_) /
//       / /| | / __ \/ __  / ___/ _ \/ __ `/ ___/  / /_/ / /_/ _ \/ / / 
//      / ___ |/ / / / /_/ / /  /  __/ /_/ (__  )  / ____/ __/  __/ / /  
//     /_/  |_/_/ /_/\__,_/_/   \___/\__,_/____/  /_/   /_/  \___/_/_/
//                                                          
//  Product:     cubeSQL.NET - Wrapper for the cubeSQL C SDK database driver
//  Version:     Revision: 1.0.0, Build: 1
//  Date:        2021/06/03 21:58:48
//  Author:      Andreas Pfeil <patreon@familie-pfeil.com>
//
//  Description: .NET wrapper for the cubeSQL database client driver based 
//               on Marco Bambini's C SDK.
//
//  Usage:       using CubeSDK;
//
//  License:     BEER license / MIT license
//
//  Copyright (C) 2021 by Andreas Pfeil
//
// -----------------------------------------------------------------------TAB=2


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cubeSDK {

  using System;
  using System.Runtime.InteropServices;

  class CubeSQLException : Exception {
    private int number = -1;
    public CubeSQLException() : base( "Unknown" ) { }
    public CubeSQLException( string message ) : base( message ) { }
    public CubeSQLException( string message, Exception inner ) : base( message, inner ) {}
    public CubeSQLException( string message, int number ) : base( message ) { this.number = number; }
    public string toString() { return String.Format( "{0} ({1})", Message, number ); }
    public int getErrorNumber() { return number; }
  }

  class cubeSDK {
    protected IntPtr db = IntPtr.Zero;

    public const int CUBESQL_DEFAULT_PORT    = 4430;
    public const int CUBESQL_DEFAULT_TIMEOUT = 12;
    public const int kTRUE                   = 1;
    public const int kFALSE                  = 0;

    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern Int64 cubesql_changes( IntPtr db );

    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern int cubesql_send_data( IntPtr db, string buffer, int length );
    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern int cubesql_send_enddata( IntPtr db );
    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern string cubesql_receive_data( IntPtr db, out int length, out int isEndChunk );


    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern int cubesql_bind( IntPtr db, string sql, out string colValue, out int colSize, out int colType, int ncols );
    public const int CUBESQL_BIND_INTEGER  = 1;
    public const int CUBESQL_BIND_DOUBLE   = 2;
    public const int CUBESQL_BIND_TEXT     = 3;
    public const int CUBESQL_BIND_BLOB     = 4;
    public const int CUBESQL_BIND_NULL     = 5;
    public const int CUBESQL_BIND_INT64    = 8;
    public const int CUBESQL_BIND_ZEROBLOB = 9;
    
    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern IntPtr cubesql_vmprepare( IntPtr db, string sql );
    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern int cubesql_vmbind_int( IntPtr vm, int index, int value );

    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern int cubesql_vmbind_double( IntPtr vm, int index, double value );

    //[DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    //public static extern void cubesql_trace( IntPtr db, ... );

    protected string toUTF8( string defaultEncodedString ) {
      return Encoding.UTF8.GetString( Encoding.Default.GetBytes( defaultEncodedString ) );
    }
    
    public const int CUBESQL_ENCRYPTION_NONE       = 0;
    public const int CUBESQL_ENCRYPTION_AES128     = 2;
    public const int CUBESQL_ENCRYPTION_AES192     = 3;
    public const int CUBESQL_ENCRYPTION_AES256     = 4;
    public const int CUBESQL_ENCRYPTION_SSL        = 8;
    public const int CUBESQL_ENCRYPTION_SSL_AES128 = CUBESQL_ENCRYPTION_SSL + CUBESQL_ENCRYPTION_AES128;
    public const int CUBESQL_ENCRYPTION_SSL_AES192 = CUBESQL_ENCRYPTION_SSL + CUBESQL_ENCRYPTION_AES192;
    public const int CUBESQL_ENCRYPTION_SSL_AES256 = CUBESQL_ENCRYPTION_SSL + CUBESQL_ENCRYPTION_AES256;

    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern int cubesql_connect( out IntPtr csqldb, string Host, int Port, string UserName, string Password, int TimeOut, int encryption );
    public int connect( string Host, string UserName, string Password, int Port = CUBESQL_DEFAULT_PORT, int TimeOut = CUBESQL_DEFAULT_TIMEOUT, int encryption = CUBESQL_ENCRYPTION_NONE ) {
      if( !IntPtr.Zero.Equals( db ) ) disconnect();
      int result = cubesql_connect( out db, toUTF8( Host ), Port, toUTF8( UserName ), toUTF8( Password ), TimeOut, encryption );
      if( result == CUBESQL_ERR ) throw new CubeSQLException( "Connection failed", result );
      return CUBESQL_NOERR;
    }
    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern int cubesql_connect_ssl( out IntPtr csqldb, string Host, int Port, string UserName, string Password, int TimeOut, int encryption, string SSLCertificatePath );
    public int connectSSL( string Host, string UserName, string Password, string SSLCertificatePath, int Port = CUBESQL_DEFAULT_PORT, int TimeOut = CUBESQL_DEFAULT_TIMEOUT, int encryption = CUBESQL_ENCRYPTION_NONE ) {
      if( !IntPtr.Zero.Equals( db ) ) disconnect();
      int result = cubesql_connect_ssl( out db, toUTF8( Host ), Port, toUTF8( UserName ), toUTF8( Password ), TimeOut, encryption, toUTF8( SSLCertificatePath ) );
      if( result == CUBESQL_ERR ) throw new CubeSQLException( "SSLConnection failed", result );
      return CUBESQL_NOERR;
    }
    public cubeSDK( string Host, string UserName, string Password, int Port = CUBESQL_DEFAULT_PORT, int TimeOut = CUBESQL_DEFAULT_TIMEOUT, int encryption = CUBESQL_ENCRYPTION_NONE, string sslCertificatePath = null ) {
      if( String.IsNullOrEmpty( sslCertificatePath ) ) connect( Host, UserName, Password, Port, TimeOut, encryption );
      else                                             connectSSL( Host, UserName, Password, sslCertificatePath, Port, TimeOut, encryption );
    }
    
    public const int CUBESQL_SSL_LIBRARY_PATH    = 1;
    public const int CUBESQL_CRYPTO_LIBRARY_PATH = 2;
    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern void cubesql_setpath( int type, string path ); // Set Path of cryptoLibrary
    public void setPath( int type, string path ) { cubesql_setpath( type, toUTF8( path ) ); }

    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern void cubesql_disconnect( IntPtr csqldb, int gracefully ); // kTRUE, kFALSE
    public void disconnect( int gracefully = kTRUE ) {
      if( IntPtr.Zero.Equals( db ) ) return;
      cubesql_disconnect( db, gracefully );
      db = IntPtr.Zero;
    }
    ~cubeSDK() { disconnect(); }

    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern int cubesql_set_database( IntPtr db, string dbName );
    public void use( string database, string language = "DE" ) {
      cubesql_set_database( db, toUTF8( database ) );
      cubesql_execute( db, "SET CLIENT TYPE TO '.NET Driver (P-INVOKE)';" );
      cubesql_execute( db, String.Format( "SET LANGUAGE TO {0};", language ) );
    }

    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    [return: MarshalAs( UnmanagedType.LPStr )] // From C Source Code File
    public static extern string cubesql_version();
    public string version() { return cubesql_version(); }

    public const int CUBESQL_NOERR              =  0;
    public const int CUBESQL_ERR                = -1;
    public const int CUBESQL_MEMORY_ERROR       = -2;
    public const int CUBESQL_PARAMETER_ERROR    = -3;
    public const int CUBESQL_PROTOCOL_ERROR     = -4;
    public const int CUBESQL_ZLIB_ERROR         = -5;
    public const int CUBESQL_SSL_ERROR          = -6;
    public const int CUBESQL_SSL_CERT_ERROR     = -7;
    public const int CUBESQL_SSL_DISABLED_ERROR = -8;

    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern int cubesql_errcode( IntPtr db );
    public int getErrorCode() { return cubesql_errcode( db ); }
    
    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    [return: MarshalAs( UnmanagedType.LPStr )]
    public static extern string cubesql_errmsg( IntPtr db ); // Kommt vom Server
    public string getErrorMessage() { return cubesql_errmsg( db ); }

    
    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern int cubesql_ping( IntPtr db );
    public int ping() { return cubesql_ping( db ); }

    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern int cubesql_commit( IntPtr db );
    public int commit() { return cubesql_commit( db ); }

    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern int cubesql_rollback( IntPtr db );
    public int rollback() { return cubesql_rollback( db ); }

    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern void cubesql_cancel( IntPtr db );
    public void cancel() { cubesql_cancel( db ); }

    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern Int64 cubesql_affected_rows( IntPtr db );
    public Int64 affectedRows() { return cubesql_affected_rows( db ); }

    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern Int64 cubesql_last_inserted_rowID( IntPtr db );
    public Int64 lastInsertedRowID() { return cubesql_affected_rows( db ); }

    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern int cubesql_execute( IntPtr db, string sql );
    public int execute( string sql ) { return cubesql_execute( db, toUTF8( sql ) ); }

    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern IntPtr cubesql_select( IntPtr db, string sql, int unused );
    public cubeSDKResult select( string query ) {
      IntPtr csqlc = cubesql_select( db, toUTF8( query ), kFALSE );
      if( IntPtr.Zero.Equals( csqlc ) ) throw new CubeSQLException( getErrorMessage(), getErrorCode() );
      return cubeSDKResult.create( csqlc ); 
    }
  }

  class cubeSDKResult {
    protected IntPtr csqlc = IntPtr.Zero;

    public static cubeSDKResult create( IntPtr csqlc ) { 
      cubeSDKResult result = new cubeSDKResult();
      result.csqlc = csqlc;
      return result;
    }

    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern void cubesql_cursor_free( IntPtr csqlc );
    ~cubeSDKResult() {
      cubesql_cursor_free( csqlc );
      csqlc = IntPtr.Zero;
    }

    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern int cubesql_cursor_numrows( IntPtr csqlc );
    public int getNumRows() { return cubesql_cursor_numrows( csqlc ); }

    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern int cubesql_cursor_numcolumns( IntPtr csqlc );
    public int getNumCols() { return cubesql_cursor_numcolumns( csqlc ); }

    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern int cubesql_cursor_currentrow( IntPtr csqlc );
    public int getCurrentRow() { return cubesql_cursor_currentrow( csqlc ); }

    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern int cubesql_cursor_iseof( IntPtr csqlc );
    public bool eof() { return cubesql_cursor_iseof( csqlc ) == cubeSDK.kTRUE; }

    public const int CUBESQL_Type_None		  = 0;
    public const int CUBESQL_Type_Integer	  = 1;
    public const int CUBESQL_Type_Float		  = 2;
    public const int CUBESQL_Type_Text		  = 3;
    public const int CUBESQL_Type_Blob		  = 4;
    public const int CUBESQL_Type_Boolean	  = 5;
    public const int CUBESQL_Type_Date		  = 6;
    public const int CUBESQL_Type_Time		  = 7;
    public const int CUBESQL_Type_Timestamp	= 8;
    public const int CUBESQL_Type_Currency	= 9;

    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern int cubesql_cursor_columntype( IntPtr csqlc, int index );
    public int getColumnType( int column ) { return cubesql_cursor_columntype( csqlc, column ); }

    public const int CUBESQL_CURROW = -1;
    public const int CUBESQL_COLNAME = 0;
    public const int CUBESQL_COLTABLE = -2;
    public const int CUBESQL_ROWID = -666;

    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern IntPtr cubesql_cursor_field( IntPtr csqlc, int row, int column, out int length ); // Pointer auf nicht 0-terminierenden String buffer
    public byte[] getFieldAsByteArray( int row, int column ) {
      int     length = 0;
      IntPtr  result = cubesql_cursor_field( csqlc, row, column, out length );
      byte[]  data   = new byte[ length ];

      for( int i = 0; i < length; i++ ) data[ i ] = Marshal.ReadByte( result, i );

      return data;
    }
    public string getFieldAsString( int row, int column ) {
      return System.Text.Encoding.UTF8.GetString( getFieldAsByteArray( row, column ) );
    }


    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern Int64 cubesql_cursor_rowid( IntPtr csqlc, int row );
    public Int64 getRowID( int row ) { return cubesql_cursor_rowid( csqlc, row ); }
    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern Int64 cubesql_cursor_int64( IntPtr csqlc, int row, int column, Int64 defaultValue ); // liest einen Wert und wandelt ihn in ein  int64, falls kein wert gefunden wurde, wird defaultValue zurückgegeben
    public Int64 getInt64Value( int row, int column, Int64 defaultValue ) { return cubesql_cursor_int64( csqlc, row, column, defaultValue ); }
    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern int cubesql_cursor_int( IntPtr csqlc, int row, int column, int defaultValue );
    public int getIntValue( int row, int column, int defaultValue ) { return cubesql_cursor_int( csqlc, row, column, defaultValue ); }
    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern double cubesql_cursor_double( IntPtr csqlc, int row, int column, double defaultValue );
    public double getDoubleValue( int row, int column, double defaultValue ) { return cubesql_cursor_double( csqlc, row, column, defaultValue ); }

    public const int CUBESQL_SEEKNEXT = -2;
    public const int CUBESQL_SEEKFIRST= -3;
    public const int CUBESQL_SEEKLAST = -4;
    public const int CUBESQL_SEEKPREV = -5;

    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern int cubesql_cursor_seek( IntPtr csqlc, int index );
    public int seek( int index ) { return cubesql_cursor_seek( csqlc, index ); }

    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern IntPtr cubesql_cursor_cstring( IntPtr csqlc, int row, int column );
    //[return: MarshalAs( UnmanagedType.LPStr )]
    //public static extern string cubesql_cursor_cstring( IntPtr csqlc, int row, int column ); // wer bibt den calloc speicher frei?
    // Pointer auf calloc Speicher der noch freigegeben werdenmuß (string str = Marshal.PtrToStringAuto(ptr);)
    // Pointer zeigt auf Daten, diese sind UTF8, UTF16, UTF16le, UTF16be
    /*
     *   string retStr = CubeSQL.cubesql_cursor_cstring( csqlc, CubeSQL.CUBESQL_CURROW, i );
          //string retStr = Marshal.PtrToStringAuto( ret );
          string retStr = Marshal.PtrToStringAnsi( ret );
          //Marshal.FreeCoTaskMem( ret ); // crash
     */

    [DllImport( "cubesql.dll", CallingConvention = CallingConvention.Cdecl )]
    public static extern IntPtr cubesql_cursor_cstring_static( IntPtr csqlc, int row, int column, byte[] buffer, int bufferLength );
    /*
       byte[] data = new byte[ 512 ];
       string cc = String.Empty;
       IntPtr cp = CubeSQL.cubesql_cursor_cstring_static( csqlc, CubeSQL.CUBESQL_CURROW, i, data, data.Length );
       if( !IntPtr.Zero.Equals( cp ) ) cc = System.Text.Encoding.UTF8.GetString( data, 0, Array.IndexOf( data, (byte)0 ) );
     */
  }

}