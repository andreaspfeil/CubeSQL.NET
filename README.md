# CubeSQL.NET
Feature complete C# wrapper to Marco Bambinis nativ cubeSQL C SDK.

## Usage example

```c#
namespace CubeSQLTestApp {

  using CubeSQL;

  class CubeSQLTestApp {
    static void Main( string[] args ) {
      CubeSQL c = new CubeSQL( "localhost", "loginname", "password" );

      Console.WriteLine( "Version = " + c.version() );
      c.use( "demo" );

      CubeSQLResult result = c.select( "SELECT * FROM Feedback" );
      Console.WriteLine( "ErrorCode    = " + c.getErrorCode() );
      Console.WriteLine( "ErrorMessage = " + c.getErrorMessage() );
      Console.WriteLine( "Rows        = " + result.getNumRows() );
      Console.WriteLine( "Columns     = " + result.getNumCols() );
      Console.WriteLine( "Current Row = " + result.getCurrentRow() );

      while( result.hasMoreRows() ) {
        for( int I = result.getNumCols(), i = 1; i <= I; i++ ) {
          Console.Write( "Column " + i + " (" + result.getColumnName( i ) + "): " );
          Console.WriteLine( result.getStringValue( i ) );
        }
        result.next();
      }
    }
  }
}
```

## Installation

## Documentation

- [Wiki](https://github.com/andreaspfeil/CubeSQL.NET/wiki)

## Video Tutorials

- [YouTube](https://www.youtube.com/channel/UCQF_wTmbR5aJZUcb7U1_0Fw)

## Donate

- [github](https://github.com/sponsors/andreaspfeil)
- [Patreon](https://www.patreon.com/andreas_pfeil)
- [PayPal](https://www.paypal.com/paypalme/PfeilAndreas/10.00EUR)

## Contributors

- [Marco Bambini](https://github.com/marcobambini) (Author of cubeSQL and the original nativ client SDK)

## Acknowledgments

- [cubeSQL](https://sqlabs.com/cubesql)

## See also

- [cubeSQL.Python2](https://github.com/andreaspfeil/CubeSQL.Python2)
- [cubeSQL.Python3](https://github.com/andreaspfeil/CubeSQL.Python3)
- [cubeSQL.go](https://github.com/andreaspfeil/CubeSQL.go)

## License

[BEER license / MIT license](https://github.com/andreaspfeil/CubeSQL.NET/blob/main/LICENSE) 

The BEER license is basically the same as the MIT license [(see link)](https://github.com/andreaspfeil/CubeSQL.NET/blob/main/LICENSE), except 
that you should buy the author a beer [(see Donate)](https://github.com/andreaspfeil/CubeSQL.NET#donate) if you use this software.

## Sponsors

none yet - YOU can still be number one in this list!!!

