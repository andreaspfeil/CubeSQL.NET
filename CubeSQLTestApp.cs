//
//         ___              __                        ____  ____     _ __
//        /   |  ____  ____/ /_______  ____ ______   / __ \/ __/__  (_) /
//       / /| | / __ \/ __  / ___/ _ \/ __ `/ ___/  / /_/ / /_/ _ \/ / / 
//      / ___ |/ / / / /_/ / /  /  __/ /_/ (__  )  / ____/ __/  __/ / /  
//     /_/  |_/_/ /_/\__,_/_/   \___/\__,_/____/  /_/   /_/  \___/_/_/
//                                                          
//  Product:     cubeSQL.NET - Demo App for the C-SDK .NET wrapper
//  Version:     Revision: 1.0.0, Build: 1
//  Date:        2021/06/03 21:58:48
//  Author:      Andreas Pfeil <patreon@familie-pfeil.com>
//
//  Description: Opens a cubeSQL database connection with the help of the
//							 Marco Bambini's native C-SDK driver, selects a database 
//							 and selects some rows. Outputs info and the rows.
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

namespace CubeSQLTestApp {

  using CubeSQL;

  class CubeSQLTestApp {
    static void Main( string[] args ) {
      CubeSQL c = new CubeSQL( "localhost", "logingname", "password" );

      Console.WriteLine( "Version = " + c.version() );
      c.use( "webuy.coupons" );

      CubeSQLResult result = c.select( "SELECT * FROM Feedback" );
      Console.WriteLine( "ErrorCode    = " + c.getErrorCode() );
      Console.WriteLine( "ErrorMessage = " + c.getErrorMessage() );
      Console.WriteLine( "Rows        = " + result.getNumRows() );
      Console.WriteLine( "Columns     = " + result.getNumCols() );
      Console.WriteLine( "Current Row = " + result.getCurrentRow() );

      while( result.hasMoreRows() ) {
        foreach( string columnName in result.columnNames ) 
					Console.WriteLine( String.Format( "Spalte {0} = {1}", columnName, result[ columnName ] ) );

        for( int I = result.getNumCols(), i = 1; i <= I; i++ ) {
          Console.Write( "Spalte " + i + " (" + result.getColumnName( i ) + "): " );
          Console.WriteLine( result.getStringValue( i ) );
        }
        result.next();
      }
    }
  }
}