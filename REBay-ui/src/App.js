import React, { Component } from 'react';
import Search from './components/Search';
import AppFooter from './components/AppFooter';

class App extends Component {
  render() {

    return (
      <>
       <Search></Search>
        
       <AppFooter></AppFooter>
       </>
       
    );
  }
}



export default App;