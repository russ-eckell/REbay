import React, { Component } from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faSearch, faMoneyBillWave, faArrowsAltH, faSkull, faPercentage, faArrowDown, faBan } from '@fortawesome/free-solid-svg-icons';
import logo from '../images/rebay_logo.png';
import axios from 'axios';
import {
    Navbar,
    NavbarBrand,
    Input,
    Button,
    NavbarText,
} from 'reactstrap';
import '../App.css';

class Search extends Component {
    constructor(props) {
        super(props);
        this.state = {
            keywords: '',
            summaryData: {},
            completedItemsData: {},
            loading: false,
            message: '',
        };
        this.cancel = '';
    };

    componentDidMount(){
        const input = document.querySelector("#search");
        input.focus();
     }

    handleKeyPress = (event) => {
        if (event.key === 'Enter'){
          const searchButton = document.querySelector("#btn-search");
          searchButton.focus();
          this.handleSearchButtonClick(this.state);
        }
      }

    // will keep our keywords up to date
    handleSearchChange = (event) => {
        const keywords = event.target.value;
            this.setState({ keywords, loading: true, message: ''  } );
    };
    
    handleSearchButtonClick = ( state ) => {
        if (state.keywords !=="") {
            this.fetchSearchResults(state.keywords);
        }
    };

    handleClearButtonClick = () => {
        this.setState({
            keywords:"",
            summaryData: {},
            completedItemsData: {},
            loading: false,
            message: ""
        });     
        
        const input = document.querySelector("#search");
        input.focus();
    };

    fetchSearchResults = (keywords) => {
        const productionIP = "71.254.201.121";
        let searchUrl;
        if (window.location.hostname === "localhost" || window.location.hostname === "127.0.0.1") {
            searchUrl = `https://localhost:44392/api/items?keywordSearch=${keywords}`;
        } else {
            searchUrl = `http://${productionIP}/REbayAPI/api/items?keywordSearch=${keywords}`; // My local IP.
        }
        
        //Call to fetch using axios
        // Axios provides CancelToken functionality so we don't continue to do multiple fetches if the button is hit multiple times or 
        // the seach term is changed (if we were doing a realtime search with no actual button). 
        if (this.cancel) {
            this.cancel.cancel(); // Cancel the currently running token.
        }
        this.cancel = axios.CancelToken.source(); // Create a new token.

        axios.get(searchUrl, {
            cancelToken: this.cancel.token
        })
            .then(response => {
                const resultNotFoundMessage = !(Object.keys(response.data).length > 0)
                                            ? 'No completed listings found.': '';
                console.warn(resultNotFoundMessage);
                this.setState( {
                    summaryData: response.data,
                    completedItemsData: response.data.completedItems,
                    message: resultNotFoundMessage,
                    loading: false
                });
            })
            .catch (error => {
                if (axios.isCancel(error) || error) {
                    this.setState({
                        loading: false,
                        message: 'Failed to fetch the data.'
                    })
                }
            });
    };

    renderSearchResults = () => {
        const {summaryData, completedItemsData, message} = this.state;

        if (this.state.message.length > 0) {
            return(
                <div className="items-search-summary">0 listings: {message}</div>
            ) 
        }

        if (completedItemsData && Object.keys(completedItemsData).length) {
          return(  
            <div className="results-container">
                <div className="summary-container">
                    <div className="summary-line">
                        <span className="price-range"> {summaryData.lowestSoldPrice} <FontAwesomeIcon icon={faArrowsAltH} /> </span>
                        <span className="median-price"> <FontAwesomeIcon icon={faMoneyBillWave} />{summaryData.medianSoldPrice} </span>
                        <span className="price-range"><FontAwesomeIcon icon={faArrowsAltH} /> {summaryData.highestSoldPrice} </span>
                        <span> | <FontAwesomeIcon icon={faPercentage} /> {summaryData.sellRate}  </span>
                        <span> | </span>
                        <span className="red"> <FontAwesomeIcon icon={faSkull} /><FontAwesomeIcon icon={faArrowDown} />{summaryData.lowestUnsoldPrice} </span>
                    </div>
                </div>
            
                <div className="items-search-summary">{summaryData.count} most recent completed listings</div>
                <div className="items-container">
                    {this.state.completedItemsData.map((item) => (
                        <div key={item.itemId} className="item-container d-flex align-items-top">
                            <div className="item-image-div d-inline-block">
                                <a href={item.url}><img className="item-image rounded" src={item.thumbnailUrl} alt={item.title} /></a>
                            </div>
                            <div className="item-detail d-inline-block">
                                <div className="item-end-date">
                                    <span className={GetSoldClass(item.wasSold)}>{item.endDate}</span>
                                    <span className="item-buyitnow"> | {WasBuyItNow(item.wasBuyItNow)}</span>
                                </div>
                                <div className="item-title">{item.title}</div>
                                <div className="item-condition">{item.condition}</div>
                                <div>
                                    <div className="item-price">
                                        <span className={GetSoldClass(item.wasSold)}>{item.itemPrice}</span> 
                                        <span className="item-shipping"> {item.shippingPrice}</span>
                                    </div>
                                    
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            </div>
            )
        }
      }

    render() {
        const {keywords} = this.state;
        return (
            <div className="navbar-container">
                <Navbar color="dark sticky-top">
                    <NavbarBrand>
                        <img src={logo} className="d-inline-block align-center" alt="REbay logo" />
                    </NavbarBrand>
                    <NavbarText>
                    <div className="input-group">
                        <Input 
                            type="text" 
                            name="search" 
                            id="search" 
                            value={keywords} 
                            onChange={this.handleSearchChange}
                            onKeyPress={this.handleKeyPress}
                            autoComplete="off"
                            autoCorrect="off"
                            autoCapitalize="off"
                            spellCheck="false"
                            autoFocus
                        />
                        <Button id="btn-search" onClick={() => this.handleSearchButtonClick(this.state)} className="search-button">
                            <FontAwesomeIcon icon={faSearch} className="green" />
                        </Button>
                        <Button id="btn-clear" onClick={() => this.handleClearButtonClick(this.state)} className="search-button">
                            <FontAwesomeIcon icon={faBan} className="red" />
                        </Button>
                    </div>
                    </NavbarText>
                </Navbar>
                <div className = 'results-container'>
                    { this.renderSearchResults() }
                </div>
            </div>
        )
    }
}

function WasBuyItNow(wasBuyItNow) { 
    if (wasBuyItNow) {
        return <span>Buy-It-Now</span>;
    }
    return  <span>Auction</span>;
}


function GetSoldClass(wasSold) {
    if (wasSold) {
        return "was-sold";
    } else {
        return "was-not-sold";
    }
}

export default Search; 