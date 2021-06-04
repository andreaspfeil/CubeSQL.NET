//
//         ___              __                        ____  ____     _ __
//        /   |  ____  ____/ /_______  ____ ______   / __ \/ __/__  (_) /
//       / /| | / __ \/ __  / ___/ _ \/ __ `/ ___/  / /_/ / /_/ _ \/ / / 
//      / ___ |/ / / / /_/ / /  /  __/ /_/ (__  )  / ____/ __/  __/ / /  
//     /_/  |_/_/ /_/\__,_/_/   \___/\__,_/____/  /_/   /_/  \___/_/_/
//                                                          
//  Product:     cubeSQL.NET - cubeSQL database driver for .NET
//  Version:     Revision: 1.0.0, Build: 1
//  Date:        2021/06/03 21:58:48
//  Author:      Andreas Pfeil <patreon@familie-pfeil.com>
//
//  Description: .NET client class for the cubeSQL database client.
//
//  Usage:       using CubeSQL;
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

namespace CubeSQL {
  using cubeSDK;

  class CubeSQL : CubeSDK {
    protected bool autoCommit = true;

    public CubeSQL( string Host, string UserName, string Password, int Port = CUBESQL_DEFAULT_PORT, int TimeOut = CUBESQL_DEFAULT_TIMEOUT, int encryption = CUBESQL_ENCRYPTION_NONE, string sslCertificatePath = null ) : base( Host, UserName, Password, Port, TimeOut, encryption, sslCertificatePath ) {}

    public void setAutoCommit( bool autoCommit = true ) {
      this.autoCommit = autoCommit;
      execute( String.Format( "SET AUTOCOMMIT TO {0}", ( this.autoCommit ? "ON" : "OFF" ) ) );
    }
    public void beginnTransaction() {
      if( autoCommit ) execute( "SET AUTOCOMMIT TO OFF;" );
                       execute( "BEGIN DEFERRED TRANSACTION;" );
    }
    public void commitTransaction() {
                       execute( "COMMIT TRANSACTION;" );
      if( autoCommit ) execute( "SET AUTOCOMMIT TO ON;" );
    }
    public void rollBackTransaction() {
                       execute( "ROLLBACK TRANSACTION;" );
      if( autoCommit ) execute( "SET AUTOCOMMIT TO ON;" );
    }

    public bool isError() { return getErrorCode() != CUBESQL_NOERR; }
    public new CubeSQLResult select( string query ) {
      IntPtr csqlc = cubesql_select( db, toUTF8( query ), kFALSE );
      if( IntPtr.Zero.Equals( csqlc ) ) throw new CubeSQLException( getErrorMessage(), getErrorCode() );
      return CubeSQLResult.create( csqlc );
    }
    public CubeSQLResult selectSingleRow( string query ) {
      CubeSQLResult result = select( query );
      switch( result.getNumRows() ) {
        case 0: throw new CubeSQLException( "Not enough Rows", -1 );
        case 1: return result;
        default: throw new CubeSQLException( "Too many Rows", -2 );
      }
    }
    public string selectSingleStringValue( string query ) {
      CubeSQLResult result = selectSingleRow( query );
      switch( result.getNumCols() ) {
        case 0: throw new CubeSQLException( "Not enough Columns", -1 );
        case 1: return result.getStringValue( 1 );
        default: throw new CubeSQLException( "Too many Columns", -2 );
      }
    }
    public int selectSingleIntegerValue( string query ) { return Int32.Parse( selectSingleStringValue( query ) ); }
    public Int64 selectSingleInteger64Value( string query ) { return Int64.Parse( selectSingleStringValue( query ) ); }
    public Double selectSingleDoubleValue( string query ) { return Double.Parse( selectSingleStringValue( query ) ); }
    public bool selectSingleBooleanValue( string query ) {
      var val = selectSingleStringValue( query );
      return Int32.Parse( val ) == 1 || Boolean.Parse( val );
    }
  }

  class CubeSQLResult : CubeSQLServerResult {
    public string[] columNames;

    public CubeSQLResult( IntPtr csqlc ) {
      this.csqlc = csqlc;

      columNames = new string[ getNumCols() ];
      for( int i = 1, columns = columNames.Length; i <= columns; i++ ) columNames[ i - 1 ] = getColumnName( i );
    }
    public static new CubeSQLResult create( IntPtr csqlc ) { return new CubeSQLResult( csqlc ); }

    public string getColumnName( int column ) { return getFieldAsString( CubeSQLServerResult.CUBESQL_COLNAME, column ); }
    public int getColumn( string columnName ) { return 1 + Array.IndexOf( columNames, columnName ); }
    public string getStringValue( int column ) { return getFieldAsString( CubeSQLServerResult.CUBESQL_CURROW, column ); }
    public string this[ int column ] { get { return getStringValue( column ); } }
    public string this[ string columnName ] { get { return getStringValue( getColumn( columnName ) ); } }
    public bool getBooleanValue( int column ) {
      var val = getStringValue( column );
      return Int32.Parse( val ) == 1 || Boolean.Parse( val );
    }

    public int rewind() { return seek( CUBESQL_SEEKFIRST ); }
    public int next() { return seek( CUBESQL_SEEKNEXT ); }
    public int previous() { return seek( CUBESQL_SEEKPREV ); }
    public int last() { return seek( CUBESQL_SEEKLAST ); }

    public bool hasMoreRows() { return !eof(); }
  }
}