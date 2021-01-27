import React, { Fragment } from 'react';
import "../App.css";
class AppFooter extends React.Component {
    render() {
        return <Fragment>
            <footer className="navbar fixed-bottom footer">
                <span className="copyright">© {(new Date().getFullYear())} RUSSTECK</span>
            </footer>
        </Fragment>;
    }
}
export default AppFooter;